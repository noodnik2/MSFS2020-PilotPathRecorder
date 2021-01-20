﻿using System;
using System.Collections.Generic;

namespace FS2020PlanePath
{

    public class LiveCamRegistry : IRegistry<KmlLiveCam>
    {

        private IRegistry<KmlLiveCam> cacheRegistry;
        private IRegistry<LiveCamEntity> persistentRegistry;

        public LiveCamRegistry(IRegistry<LiveCamEntity> persistentRegistry)
        {
            cacheRegistry = new InMemoryRegistry<KmlLiveCam>();
            this.persistentRegistry = persistentRegistry; ;
        }

        public KmlLiveCam LoadByAlias(string alias)
        {
            KmlLiveCam kmlLiveCam;

            // if we've already got it, 
            if (TryGetById(alias, out kmlLiveCam))
            {
                // then return it
                return kmlLiveCam;
            }

            // load it from the persistence provider
            LiveCamEntity liveCamEntity;
            if (persistentRegistry.TryGetById(alias, out liveCamEntity))
            {
                // load that into a liveCam using the current URL
                kmlLiveCam = new KmlLiveCam(
                    liveCamEntity.CameraTemplate,
                    liveCamEntity.LinkTemplate
                );
                // save it (back) to the cache
                cacheRegistry.Save(alias, kmlLiveCam);
                return kmlLiveCam;
            }

            // if not found in the persistence provider, use a default one
            kmlLiveCam = DefaultLiveCam(alias);
            // and persist that for the alias
            Save(alias, kmlLiveCam);
            return kmlLiveCam;
        }

        public bool TryGetById(string alias, out KmlLiveCam kmlLiveCam)
        {
            return cacheRegistry.TryGetById(alias, out kmlLiveCam);
        }

        public bool Save(string alias, KmlLiveCam kmlLiveCam)
        {
            LiveCamEntity liveCamEntity = new LiveCamEntity
            {
                CameraTemplate = kmlLiveCam.Camera.Template,
                LinkTemplate = kmlLiveCam.Link.Template
            };
            cacheRegistry.Save(alias, kmlLiveCam);
            return persistentRegistry.Save(alias, liveCamEntity);
        }

        public bool Delete(string alias)
        {
            return cacheRegistry.Delete(alias) && persistentRegistry.Delete(alias);
        }

        public List<string> GetIds(int maxCount = -1)
        {
            return persistentRegistry.GetIds(maxCount);
        }

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

        public static bool IsDefaultDefinition(KmlLiveCam kmlLiveCam)
        {
            return (
                kmlLiveCam.Camera.Template == LiveCamConstants.DefaultCameraKmlTemplate
             && kmlLiveCam.Link.Template == LiveCamConstants.DefaultNetworkLinkKmlTemplate   
            );
        }

        public static KmlLiveCam DefaultLiveCam(string alias)
        {
            return(
                new KmlLiveCam(
                    LiveCamConstants.DefaultCameraKmlTemplate,
                    LiveCamConstants.DefaultNetworkLinkKmlTemplate
                )
            );
        }

        private static class LiveCamConstants
        {

            public const string DefaultCameraKmlTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
<kml xmlns = 'http://www.opengis.net/kml/2.2' xmlns:gx='http://www.google.com/kml/ext/2.2' xmlns:kml='http://www.opengis.net/kml/2.2' xmlns:atom='http://www.w3.org/2005/Atom'>
  <NetworkLinkControl>
    <Camera>
      <longitude>{longitude}</longitude>
      <latitude>{latitude}</latitude>
      <altitude>{altitude}</altitude>
      <heading>{heading}</heading>
      <tilt>{tilt}</tilt>
      <roll>{roll}</roll>
      <altitudeMode>absolute</altitudeMode>
    </Camera>
  </NetworkLinkControl>
</kml>";

            public const string DefaultNetworkLinkKmlTemplate =
    @"<?xml version='1.0' encoding='UTF-8'?>
<kml xmlns='http://www.opengis.net/kml/2.2' xmlns:gx='http://www.google.com/kml/ext/2.2' xmlns:kml='http://www.opengis.net/kml/2.2' xmlns:atom='http://www.w3.org/2005/Atom'>
<NetworkLink>
	<name>MSFS2020-PilotPathRecorder Live Camera ({alias})</name>
	<flyToView>1</flyToView>
	<Link>
		<href>{url}</href>
        <!-- onChange, onInterval, ... -->
		<refreshMode>onChange</refreshMode>
        <!-- number of seconds between refreshs -->
		<refreshInterval>1</refreshInterval>
        <!-- never, onStop, onRequest ... -->
		<viewRefreshMode>onStop</viewRefreshMode>
        <!-- number of seconds after camera stops -->
		<viewRefreshTime>0</viewRefreshTime>
	</Link>
</NetworkLink>
</kml>";

        }

    }

    public class LiveCamEntity
    {
        public string CameraTemplate { get; set; }
        public string LinkTemplate { get; set; }
    }

}
