/*
 * TheNexusAvenger
 *
 * Storage for files not in project structures.
 */

using System;
using System.IO;

namespace NexusEmbeddedEditor.Project
{
    public class TemporaryFiles
    {
        private string TemporaryDirectory;
        
        /*
         * Creates a temporary files object.
         */
        public TemporaryFiles()
        {
            this.TemporaryDirectory = Path.Combine(Path.GetTempPath(),Path.GetTempFileName().Replace(".",""));
            Directory.CreateDirectory(this.TemporaryDirectory);
        }
        
        /*
         * Returns the file location for a script path.
         */
        public string GetFileLocation(string path)
        {
            // Return the file in the temporary directory if the path is a single instance.
            if (!path.Contains("."))
            {
                return Path.Combine(this.TemporaryDirectory,path);
            }
            
            // Create the temporary directory of the parent path and return the file in the path.
            var parentPath = path.Substring(0,path.LastIndexOf(".", StringComparison.Ordinal));
            Directory.CreateDirectory(Path.Combine(this.TemporaryDirectory,parentPath));
            return Path.Combine(this.TemporaryDirectory,parentPath,path.Substring(path.LastIndexOf(".",StringComparison.Ordinal) + 1) + ".lua");
        }
    }
}