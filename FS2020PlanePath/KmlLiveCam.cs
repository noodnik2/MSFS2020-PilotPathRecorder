﻿using System.Collections.Generic;
using System.Linq;

namespace FS2020PlanePath
{

    public class KmlLiveCam : ILiveCam<KmlCameraParameterValues, KmlNetworkLinkValues>
    {

        public static TemplateRendererFactory TemplateRendererFactory { get; } = new TemplateRendererFactory(
            (message, details) => $"<rendererError message='{message}' details='{details}' />"
        );

        public KmlLiveCam(string cameraTemplate, string linkTemplate)
        {
            _camera = TemplateRendererFactory.newTemplateRenderer<KmlCameraParameterValues>(cameraTemplate);
            _link = TemplateRendererFactory.newTemplateRenderer<KmlNetworkLinkValues>(linkTemplate);
        }

        public IStringTemplateRenderer<KmlCameraParameterValues> Camera { get { return _camera; } }

        public IStringTemplateRenderer<KmlNetworkLinkValues> Link { get { return _link; } }

        public string[] Diagnostics
        {
            get
            {
                return Camera.Diagnostics.Concat(Link.Diagnostics).ToArray();
            }
        }

        private IStringTemplateRenderer<KmlCameraParameterValues> _camera;
        private IStringTemplateRenderer<KmlNetworkLinkValues> _link;

    }

    public delegate KmlCameraParameterValues[] GetMultitrackUpdatesDelegate(int flightId, long sinceSeq);

    // see: https://developers.google.com/kml/documentation/kmlreference#camera
    public class KmlCameraParameterValues
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        public double altitude { get; set; }    // meters
        public double heading { get; set; }
        public double tilt { get; set; }
        public double roll { get; set; }
        public long seq { get; set; }    // update sequence
        public int flightId { get; set; }
        public GetMultitrackUpdatesDelegate getMultitrackUpdates { get; set; }
        public Dictionary<string, string> query { get; set; }

    }

    // see: https://developers.google.com/kml/documentation/kmlreference#networklink
    public class KmlNetworkLinkValues
    {
        public string url { get; set; }
        public string alias { get; set; }

    }

    /** A "Live Cam" has a "Camera", which can be "called back" repeatedly through a "Link" */
    public interface ILiveCam<CV, LV>
    {

        /** A "Camera" renders a snapshot using current values */
        IStringTemplateRenderer<CV> Camera { get; }

        /** A "Link" renders a callback to the "Camera" */
        IStringTemplateRenderer<LV> Link { get; }

        string[] Diagnostics { get; }

    }

}
