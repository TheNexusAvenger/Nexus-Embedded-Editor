/*
 * TheNexusAvenger
 *
 * Stores information for Rojo 0.5 and newer.
 */

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NexusEmbeddedEditor.Project
{
    public class RojoJsonStructure
    {
        public Dictionary<string,object> tree;
    }
    
    public class Rojo: IProjectStructureInformation
    {
        private string ProjectDirectory;
        private string ProjectFileLocation;
        
        /*
         * Creates a Rojo project structure information object.
         */
        public Rojo(string directory,string[] args)
        {
            this.ProjectDirectory = directory;

            // Set the project file.
            if (args.Length > 0)
            {
                this.ProjectFileLocation = args[0];
            }
            else
            {
                this.ProjectFileLocation = Path.Combine(this.ProjectDirectory,"default.project.json");
            }
        }
        
        /*
         * Returns the name of the project structure.
         */
        public string GetName()
        {
            return "Rojo";
        }
        
        /*
         * Returns if a structure is valid for the directory.
         */
        public bool IsProjectStructureValid()
        {
            return File.Exists(this.ProjectFileLocation);
        }
        
        /*
         * Returns the paths for a given tree object.
         */
        public HashSet<string> GetPaths(Dictionary<string,object> rojoNode)
        {
            // Process the subnodes.
            var paths = new HashSet<string>();
            foreach (var node in rojoNode)
            {
                if (node.Key.ToLower() == "$path")
                {
                    paths.Add((string) node.Value);
                } else if (!node.Key.StartsWith("$"))
                {
                    foreach (var path in this.GetPaths(JsonConvert.DeserializeObject<Dictionary<string,object>>(node.Value.ToString())))
                    {
                        paths.Add(path);
                    }
                }
            }
            
            // Return the the paths.
            return paths;
        }
        
        /*
         * Returns the files and directories to search through for a file.
         */
        public List<string> GetSearchDirectories()
        {
            // Get the paths.
            var paths = new List<string>();
            foreach (var path in GetPaths(JsonConvert.DeserializeObject<RojoJsonStructure>(File.ReadAllText(this.ProjectFileLocation)).tree))
            {
                paths.Add(path);
            }
            
            // Return the paths.
            return paths;
        }
    }
}