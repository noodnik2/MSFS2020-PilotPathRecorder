using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FS2020PlanePath
{

    public class LiveCamServer
    {

        /// <param name="liveCamUrl">URL link to a liveCam</param>
        /// <returns>alias of the liveCam referenced by 'liveCamUrl'</returns>
        /// <exception cref="UriFormatException">invalid liveCam URL</exception>
        public static string UrlToAlias(string liveCamUrl)
        {
            string liveCamPath = HttpListener.GetPath(liveCamUrl);
            if (!liveCamPath.StartsWith(liveCamUrlPathPrefix))
            {
                throw new UriFormatException($"invalid liveCam URL({liveCamUrl})");
            }
            return liveCamPath.Remove(0, liveCamUrlPathPrefix.Length);
        }

        /// <param name="alias">alias identifying the liveCam</param>
        /// <returns>URL link to the liveCam identified by 'alias'</returns>
        public static string AliasToUrl(string alias)
        {
            return $"{urlPrefix}{liveCamUrlPathPrefix}{alias}";
        }

        /// <param name="scKmlAdapter">Server state / context</param>
        /// <param name="liveCamRegistry">Registry of liveCam instances referenced by the server</param>
        public LiveCamServer(
            ScKmlAdapter scKmlAdapter,
            LiveCamRegistry liveCamRegistry
        )
        {

            this.scKmlAdapter = scKmlAdapter;
            this.liveCamRegistry = liveCamRegistry;
            
            routingTable = new List<Route>();

            // route liveCam requests
            routingTable.Add(
                new Route
                {
                    IsSupported = request => request.path.StartsWith(liveCamUrlPathPrefix),
                    Handler = request => handleLiveCamRequest(request)
                }
            );
            // route (intended to be diagnostic / development) evaluate template requests
            routingTable.Add(
                new Route
                {
                    IsSupported = request => request.path.StartsWith(evalTemplatePathPrefix),
                    Handler = request => handleInternalRequest(request)
                }
            );
            // route all other requests (just return specified resource)
            routingTable.Add(
                new Route
                {
                    IsSupported = request => true,
                    Handler = request => handleResourceRequest(request)
                }
            );

        }

        /// <summary>
        /// Stops the server, if started.  
        /// If not started, does nothing.
        /// </summary>
        public void Stop()
        {
            if (activeLinkListener != null)
            {
                try
                {
                    activeLinkListener.Dispose();
                } finally {
                    activeLinkListener = null;
                }                                
            }
        }

        /// <summary>
        /// Starts the server.
        /// Will first stop any running server.
        /// </summary>
        /// <param name="hostUri"></param>
        public void Start(Uri hostUri)
        {

            if (activeLinkListener != null)
            {
                Stop();
            }

            HttpListener linkListener = null;
            try
            {
                linkListener = new HttpListener(hostUri, request => handleServerRequest(request));
                linkListener.Enable();
                activeLinkListener = linkListener;
            }
            catch (SystemException ene)
            {
                if (linkListener != null)
                {
                    linkListener.Dispose();
                }
                throw ene;
            }

        }

        private byte[] handleServerRequest(HttpListener.Request request)
        {
            foreach (Route route in routingTable)
            {
                if (route.IsSupported(request))
                {
                    Console.WriteLine($"handling path({request.path})");
                    return route.Handler(request);
                }
            }
            return s2b(KmlLiveCam.TemplateRendererFactory.rendererErrorHandler("request ignored", $"no route found for({request.path}"));
        }

        private byte[] handleLiveCamRequest(HttpListener.Request request)
        {
            scKmlAdapter.KmlCameraValues.query = request.query;

            string alias = request.path.Remove(0, liveCamUrlPathPrefix.Length); ;
            KmlLiveCam liveCam;
            if (liveCamRegistry.TryGetById(alias, out liveCam))
            {
                return s2b(liveCam.Camera.Render(scKmlAdapter.KmlCameraValues));
            }

            return s2b(KmlLiveCam.TemplateRendererFactory.rendererErrorHandler("request ignored", $"LiveCam({alias}) not found"));
        }

        private byte[] handleInternalRequest(HttpListener.Request request)
        {
            Console.WriteLine($"handling internal request({request.path})");
            scKmlAdapter.KmlCameraValues.query = request.query;
            string template = Encoding.UTF8.GetString(request.GetBody());
            return s2b(
                KmlLiveCam.TemplateRendererFactory.newTemplateRenderer<KmlCameraParameterValues>(template)
                .Render(scKmlAdapter.KmlCameraValues)
            );
        }

        private byte[] handleResourceRequest(HttpListener.Request request)
        {
            string resourceName = request.path;
            string resourceFileName = $"Resources/{resourceName}";
            Console.WriteLine($"reading resource({resourceFileName})");
            try
            {
                return File.ReadAllBytes(resourceFileName);
            }
            catch (Exception ex)
            {
                return s2b(KmlLiveCam.TemplateRendererFactory.rendererErrorHandler($"resource({resourceName}) not available", ex.Message));
            }
        }

        private byte[] s2b(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }

        class Route
        {
            public Func<HttpListener.Request, bool> IsSupported;
            public Func<HttpListener.Request, byte[]> Handler;
        }

        private const string urlPrefix = "http://localhost:8000/";
        private const string liveCamUrlPathPrefix = "kmlcam/";
        private const string evalTemplatePathPrefix = "eval/template/";
        private ScKmlAdapter scKmlAdapter;
        private LiveCamRegistry liveCamRegistry;
        private HttpListener activeLinkListener;
        private List<Route> routingTable;

    }

}
