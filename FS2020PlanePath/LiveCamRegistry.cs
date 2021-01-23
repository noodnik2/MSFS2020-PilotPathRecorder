using System;
using System.Collections.Generic;

namespace FS2020PlanePath
{

    public class LiveCamRegistry : IRegistry<KmlLiveCam>
    {

        private IRegistry<KmlLiveCam> cacheRegistry;
        private IRegistry<LiveCamEntity> persistentRegistry;
        private Func<string, string> defaultCameraTemplateGetter;
        private Func<string, string> defaultLinkTemplateGetter;

        public LiveCamRegistry(
            IRegistry<LiveCamEntity> persistentRegistry,
            Func<string, string> defaultCameraTemplateGetter,
            Func<string, string> defaultLinkTemplateGetter
        )
        {
            cacheRegistry = new InMemoryRegistry<KmlLiveCam>();
            this.persistentRegistry = persistentRegistry;
            this.defaultCameraTemplateGetter = defaultCameraTemplateGetter;
            this.defaultLinkTemplateGetter = defaultLinkTemplateGetter;
        }

        public KmlLiveCam LoadByAlias(string alias)
        {
            KmlLiveCam kmlLiveCam;
            if (!TryGetById(alias, out kmlLiveCam))
            {
                kmlLiveCam = DefaultLiveCam(alias);
                Save(alias, kmlLiveCam);
            }
            return kmlLiveCam;
        }

        public bool TryGetById(string alias, out KmlLiveCam kmlLiveCam)
        {
            if (cacheRegistry.TryGetById(alias, out kmlLiveCam))
            {
                return true;
            }

            LiveCamEntity liveCamEntity;
            if (persistentRegistry.TryGetById(alias, out liveCamEntity))
            {
                kmlLiveCam = new KmlLiveCam(liveCamEntity.CameraTemplate, liveCamEntity.LinkTemplate);
                cacheRegistry.Save(alias, kmlLiveCam);
                return true;
            }

            return false;
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

        public bool IsDefaultDefinition(KmlLiveCam kmlLiveCam, string alias)
        {
            return (
                kmlLiveCam.Camera.Template == defaultCameraTemplateGetter.Invoke(alias)
             && kmlLiveCam.Link.Template == defaultLinkTemplateGetter.Invoke(alias)
            );
        }

        public KmlLiveCam DefaultLiveCam(string alias)
        {
            return(
                new KmlLiveCam(
                    defaultCameraTemplateGetter.Invoke(alias),
                    defaultLinkTemplateGetter.Invoke(alias)
                )
            );
        }

    }

    public class LiveCamEntity
    {
        public string CameraTemplate { get; set; } = "";
        public string LinkTemplate { get; set; } = "";
    }

}
