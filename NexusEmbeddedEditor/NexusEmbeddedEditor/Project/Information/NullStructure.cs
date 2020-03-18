/*
 * TheNexusAvenger
 *
 * Stores information for no project structure.
 */

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NexusEmbeddedEditor.Project.Information
{
    public class NullStructure : IProjectStructureInformation
    {
        private string ProjectDirectory;
        
        /*
         * Creates a Rojo project structure information object.
         */
        public NullStructure(string directory)
        {
            this.ProjectDirectory = directory;
        }
        
        /*
         * Returns the name of the project structure.
         */
        public string GetName()
        {
            return "No Structure";
        }
        
        /*
         * Returns if a structure is valid for the directory.
         */
        public bool IsProjectStructureValid()
        {
            return true;
        }

        /*
         * Returns the files and directories to search through for a file.
         */
        public Dictionary<string,string> GetSearchDirectories()
        {
            return new Dictionary<string,string>();
        }
    }
}