using System;
using System.IO;

namespace FS2020PlanePath
{
    public class LiveCamRegistryFactory
    {

        public LiveCamRegistry NewRegistry()
        {
            return new LiveCamRegistry(
                new JsonFilesystemRegistry<LiveCamEntity>($"{typeof(KmlLiveCam).Name}_"),
                alias => GetDefaultTemplate(alias, "-camera"),
                alias => GetDefaultTemplate(alias, "-link")
            );
        }

        private string GetDefaultTemplate(string alias, string suffix)
        {
            string defaultTemplateFilename = $"Resources/liveCam/defaults/{Uri.EscapeDataString(alias)}{suffix}";
            Console.WriteLine($"loading default template for({alias}) from({defaultTemplateFilename})");
            if (!File.Exists(defaultTemplateFilename))
            {
                Console.WriteLine($"default template for({alias}) not found({defaultTemplateFilename})");
                return "";
            }
            return File.ReadAllText(defaultTemplateFilename);
        }

    }

}
