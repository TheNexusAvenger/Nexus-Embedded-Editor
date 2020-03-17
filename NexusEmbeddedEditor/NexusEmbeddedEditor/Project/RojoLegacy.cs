/*
 * TheNexusAvenger
 *
 * Stores information for Rojo 0.4 and older.
 */

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NexusEmbeddedEditor.Project
{
    public class RojoLegacyParition
    {
        public string path;
    }
    
    public class RojoLegacyJsonStructure
    {
        public Dictionary<string,RojoLegacyParition> partitions;
    }
    
    public class RojoLegacy : IProjectStructureInformation
    {
        private string ProjectDirectory;
        
        /*
         * Creates a Rojo Legacy project structure information object.
         */
        public RojoLegacy(string directory)
        {
            this.ProjectDirectory = directory;
        }
        
        /*
         * Returns the name of the project structure.
         */
        public string GetName()
        {
            return "Rojo Legacy";
        }
        
        /*
         * Returns if a structure is valid for the directory.
         */
        public bool IsProjectStructureValid()
        {
            return File.Exists(Path.Combine(this.ProjectDirectory,"rojo.json"));
        }
        
        /*
         * Returns the files and directories to search through for a file.
         */
        public List<string> GetSearchDirectories()
        {
            // Get the paths.
            var paths = new List<string>();
            var projectFile = Path.Combine(this.ProjectDirectory,"rojo.json");
            foreach (var partition in JsonConvert.DeserializeObject<RojoLegacyJsonStructure>(File.ReadAllText(projectFile)).partitions.Values)
            {
                if (!paths.Contains(partition.path))
                {
                    paths.Add(partition.path);
                }
            }
            
            // Return the paths.
            return paths;
        }
    }
}