﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace FS2020PlanePath
{

    public class KmlLiveCam : ILiveCam, IEquatable<KmlLiveCam>
    {

        private IDictionary<string, IStringTemplateRenderer<KmlCameraParameterValues>> lensRenderers;
        private string[] lensNames;

        public static TemplateRendererFactory TemplateRendererFactory { get; } = new TemplateRendererFactory(
            (message, details) => $"<rendererError message='{message}' details='{details}' />"
        );

        public KmlLiveCam(LiveCamEntity liveCamEntity)
        {
            LiveCamLensEntity[] lens = liveCamEntity.Lens;
            lensNames = lens.Select(l => l.Name).ToArray();
            lensRenderers = lens.ToDictionary(
                l => l.Name, 
                l => TemplateRendererFactory.newTemplateRenderer<KmlCameraParameterValues>(l.Template)
            );
        }

        public IEnumerable<string> LensNames => lensNames;

        public IStringTemplateRenderer<KmlCameraParameterValues> GetLens(string lensName)
        {
            Debug.Assert(lensRenderers.ContainsKey(lensName));
            return lensRenderers[lensName];
        }

        public string[] Diagnostics => LensNames.SelectMany(lensName => GetLens(lensName).Diagnostics).ToArray();

        public bool Equals(KmlLiveCam other)
        {
            if (other == null)
            {
                return false;
            }
            if (other == this)
            {
                return true;
            }
            if ((other == null) || !GetType().Equals(other.GetType()))
            {
                return false;
            }

            KmlLiveCam kmlLiveCam = other as KmlLiveCam;
            if (!LensNames.SequenceEqual(other.LensNames))
            {
                return false;
            }

            foreach (string lensName in LensNames)
            {
                if (GetLens(lensName).Template != other.GetLens(lensName).Template)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object other)
        {
            return Equals(other as KmlLiveCam);
        }

        public override int GetHashCode()
        {
            int hashCode = -96881253;
            hashCode = hashCode * -1521134295 + EqualityComparer<IDictionary<string, IStringTemplateRenderer<KmlCameraParameterValues>>>.Default.GetHashCode(lensRenderers);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(lensNames);
            return hashCode;
        }

    }

    public delegate KmlCameraParameterValues[] GetMultitrackUpdatesDelegate(int flightId, long sinceSeq);

    //
    // see:
    //  - https://developers.google.com/kml/documentation/kmlreference#camera
    //  - https://developers.google.com/kml/documentation/kmlreference#networklink
    //
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
        public int refreshSeconds { get; set; } = 5;    // sync with "AboveThresholdWriteFreq"
        public string url { get; set; }
        public string alias { get; set; }
        public string lens { get; set; }
        public GetMultitrackUpdatesDelegate getMultitrackUpdates { get; set; }
        public Dictionary<string, string> query { get; set; }

    }

    public interface ILiveCam
    {

        IStringTemplateRenderer<KmlCameraParameterValues> GetLens(string lensName);

        IEnumerable<string> LensNames { get; }

        string[] Diagnostics { get; }

    }

}
