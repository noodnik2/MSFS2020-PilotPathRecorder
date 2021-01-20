using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FS2020PlanePath
{

    public class FilesystemRegistry<T> : IRegistry<T>
    {

        string fileNamePrefix;
        string fileNameSuffix;

        public FilesystemRegistry(string fileNamePrefix)
        {
            this.fileNamePrefix = fileNamePrefix;
            this.fileNameSuffix = ".json";
        }

        public bool TryGetById(string id, out T value)
        {
            string fileName = FilenameForId(id);
            try
            {
                using (StreamReader file = File.OpenText(fileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    value = (T) serializer.Deserialize(file, typeof(T));
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Can't load({fileName}); {ex.Message}");
                value = default(T);
                return false;
            }
        }

        public bool Save(string id, T value)
        {
            string fileName = FilenameForId(id);
            try
            {
                using (StreamWriter file = File.CreateText(fileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, value);
                }
                return true;
            } catch(Exception ex)
            {
                Console.WriteLine($"Can't save({fileName}); {ex.Message}");
                return false;
            }
        }

        public bool Delete(string id)
        {
            string fileName = FilenameForId(id);
            try
            {
                File.Delete(fileName);
                return true;
            } catch(Exception ex)
            {
                Console.WriteLine($"Can't delete({fileName}); {ex.Message}");
                return false;
            }
        }

        public List<string> GetIds(int maxCount)
        {
            // TODO use "EnumerateFiles" to limit based upon 'maxCount'
            FileInfo[] foundFiles = new DirectoryInfo(".").GetFiles(FilenameForId("*"));
            Array.Sort(foundFiles, (f1, f2) => f2.LastAccessTimeUtc.CompareTo(f1.LastAccessTimeUtc));
            List<string> ids = new List<string>();
            int fileCount = 0;
            foreach (FileInfo foundFile in foundFiles)
            {
                string foundFileName = foundFile.Name;
                string idsLeft = foundFileName.Substring(fileNamePrefix.Length);
                ids.Add(idsLeft.Remove(idsLeft.Length - fileNameSuffix.Length));
                if (maxCount >= 0 && fileCount >= maxCount)
                {
                    break;
                }
                fileCount++;
            }
            // list of ids, most recently accessed first
            return ids;
        }

        private string FilenameForId(string cleanId)
        {
            return $"{fileNamePrefix}{cleanId}{fileNameSuffix}";
        }

    }

}
