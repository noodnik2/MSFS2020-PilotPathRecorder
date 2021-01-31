using System;
using System.IO;
using System.Collections.Generic;

namespace FS2020PlanePath
{
    public class LiveCamRegistryFactory
    {

        public LiveCamRegistry NewRegistry()
        {
            return new LiveCamRegistry(
                new JsonFilesystemRegistry<LiveCamEntity>($"{typeof(LiveCamEntity).Name}_"),
                new BuiltinLiveCamRegistry()               
            );
        }

    }

    class BuiltinLiveCamRegistry : IRegistry<LiveCamEntity>
    {

        private string builtinLiveCamPathPrefix;
        private string builtinLiveCamCameraSuffix;
        private string builtinLiveCamLinkSuffix;

        public BuiltinLiveCamRegistry()
        {
            builtinLiveCamPathPrefix = "Resources/liveCam/defaults/";
            builtinLiveCamCameraSuffix = "-camera";
            builtinLiveCamLinkSuffix = "-link";
        }

        public bool TryGetById(string alias, out LiveCamEntity liveCamEntity)
        {

            string defaultCameraTemplate = GetDefaultTemplate(alias, builtinLiveCamCameraSuffix);
            string defaultLinkTemplate = GetDefaultTemplate(alias, builtinLiveCamLinkSuffix);

            if (defaultCameraTemplate == null && defaultLinkTemplate == null)
            {
                liveCamEntity = null;
                return false;
            }

            liveCamEntity = new LiveCamEntity
            {
                CameraTemplate = defaultCameraTemplate == null ? "" : defaultCameraTemplate,
                LinkTemplate = defaultLinkTemplate == null ? "" : defaultLinkTemplate
            };
            return true;
        }

        public bool Save(string alias, LiveCamEntity value)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string alias)
        {
            throw new NotImplementedException();
        }

        public List<string> GetIds(int maxCount)
        {
            // TODO use "EnumerateFiles" to limit based upon 'maxCount'
            string builtinLiveCamSearchPath = $"{builtinLiveCamPathPrefix}*{builtinLiveCamCameraSuffix}";
            FileInfo[] foundFiles = new DirectoryInfo(".").GetFiles(builtinLiveCamSearchPath);
            Array.Sort(foundFiles, (f1, f2) => f2.LastAccessTimeUtc.CompareTo(f1.LastAccessTimeUtc));
            List<string> ids = new List<string>();
            int fileCount = 0;
            foreach (FileInfo foundFile in foundFiles)
            {
                string foundFileName = foundFile.Name;
                string filesystemId = foundFileName.Remove(foundFileName.Length - builtinLiveCamCameraSuffix.Length);
                ids.Add(Uri.UnescapeDataString(filesystemId));
                if (maxCount >= 0 && fileCount >= maxCount)
                {
                    break;
                }
                fileCount++;
            }
            // list of ids, most recently accessed first
            Console.WriteLine($"retrieved({ids.Count}) default ids");
            return ids;
        }

        private string GetDefaultTemplate(string alias, string suffix)
        {
            string defaultTemplateFilename = $"{builtinLiveCamPathPrefix}{Uri.EscapeDataString(alias)}{suffix}";
            if (!File.Exists(defaultTemplateFilename))
            {
                Console.WriteLine($"default template for({alias}) not found({defaultTemplateFilename})");
                return null;
            }
            Console.WriteLine($"loading default template for({alias}) from({defaultTemplateFilename})");
            return File.ReadAllText(defaultTemplateFilename);
        }

    }

}
