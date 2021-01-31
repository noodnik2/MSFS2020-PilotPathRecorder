using System.Collections.Generic;

namespace FS2020PlanePath
{

    public class LiveCamRegistry : IRegistry<KmlLiveCam>
    {

        private IRegistry<KmlLiveCam> cacheRegistry;
        private IRegistry<LiveCamEntity> persistentRegistry;
        private IRegistry<LiveCamEntity> builtinLiveCamRegistry;

        public LiveCamRegistry(
            IRegistry<LiveCamEntity> persistentRegistry,
            IRegistry<LiveCamEntity> builtinLiveCamRegistry
        )
        {
            cacheRegistry = new InMemoryRegistry<KmlLiveCam>();
            this.persistentRegistry = persistentRegistry;
            this.builtinLiveCamRegistry = builtinLiveCamRegistry;
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

            if (builtinLiveCamRegistry.TryGetById(alias, out liveCamEntity))
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
            List<string> ids = new List<string>();
            ids.AddRange(persistentRegistry.GetIds(maxCount));
            foreach (string id in builtinLiveCamRegistry.GetIds(maxCount))
            {
                if (!ids.Contains(id))
                {
                    ids.Add(id);
                }
            }
            return ids;
        }

        public bool IsDefaultDefinition(KmlLiveCam kmlLiveCam, string alias)
        {
            LiveCamEntity builtinLiveCamEntity = DefaultLiveCamEntity(alias);
            return (
                kmlLiveCam.Camera.Template == builtinLiveCamEntity.CameraTemplate
             && kmlLiveCam.Link.Template == builtinLiveCamEntity.LinkTemplate
            );
        }

        public KmlLiveCam DefaultLiveCam(string alias)
        {
            LiveCamEntity builtinLiveCamEntity = DefaultLiveCamEntity(alias);
            return new KmlLiveCam(builtinLiveCamEntity.CameraTemplate, builtinLiveCamEntity.LinkTemplate);
        }

        private LiveCamEntity DefaultLiveCamEntity(string alias)
        {
            LiveCamEntity builtinLiveCamEntity;
            if (builtinLiveCamRegistry.TryGetById(alias, out builtinLiveCamEntity))
            {
                return builtinLiveCamEntity;
            }
            return new LiveCamEntity();
        }

    }

    public class LiveCamEntity
    {
        public string CameraTemplate { get; set; } = "";
        public string LinkTemplate { get; set; } = "";
    }

}
