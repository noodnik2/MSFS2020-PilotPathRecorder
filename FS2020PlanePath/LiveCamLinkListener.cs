using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatsonWebserver;

namespace FS2020PlanePath
{

    public class LiveCamLinkListener : IDisposable
    {

        /// <exception cref="UriFormatException">malformed url</exception>
        public static Uri ParseNetworkLink(string liveCamUrl)
        {
            return new Uri(liveCamUrl);
        }

        /// <exception cref="UriFormatException">malformed url</exception>
        public static string GetAlias(string liveCamUrl)
        {
            return ParseNetworkLink(liveCamUrl).AbsolutePath.Substring(1);
        }

        public class Request
        {
            public string path;
            public Dictionary<string, string> query;
            public Func<string> GetBody;
        }

        public LiveCamLinkListener(Uri webHostUri, Func<Request, string> pathHandler)
        {
            if (!supportedSchemes.Contains(webHostUri.Scheme)) {
                throw new ArgumentException($"unsupported URI scheme: {webHostUri.Scheme}");
            }
            this.pathHandler = pathHandler;
            this.webHostUri = webHostUri;
        }

        public void Enable()
        {
            if (server == null)
            {
                bool ssl = webHostUri.Scheme == Uri.UriSchemeHttps;
                // see: https://github.com/jchristn/WatsonWebserver/wiki/Using-SSL-on-Windows
                server = new Server(webHostUri.Host, webHostUri.Port, ssl, RequestHandler);
            }
            if (!server.IsListening) {
                server.Start();
                Console.WriteLine($"{GetType().Name} listening at({webHostUri})");
            }
        }

        public void Disable()
        {
            if (server != null)
            {
                if (server.IsListening) {
                    server.Stop();
                    Console.WriteLine($"{GetType().Name} stopped listening");
                }
            }
        }

        public void Dispose()
        {
            Disable();
            if (server != null)
            {
                server.Dispose();
                server = null;
            }
        }

        public Uri Uri {
            get {
                return webHostUri;
            }
        }

        async Task RequestHandler(HttpContext context)
        {
            Console.WriteLine($"handling request({context.Request.Url.RawWithQuery})");
            string listenerResponseBody = (
                pathHandler.Invoke(
                    new Request
                    {
                        path = context.Request.Url.RawWithoutQuery.Substring(1),
                        query = context.Request.Query.Elements,
                        GetBody = () => context.Request.DataAsString()
                    }
                )
            );
            Console.WriteLine($"listenerResponseBody({listenerResponseBody})");
            //Console.WriteLine($"listenerResponseBody.Length({listenerResponseBody.Length})");
            await context.Response.Send(listenerResponseBody);
        }

        private readonly List<string> supportedSchemes = new List<string> { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
        private readonly Func<Request, string> pathHandler;
        private readonly Uri webHostUri;

        private Server server;

    }
}
