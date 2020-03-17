/*
 * TheNexusAvenger
 * 
 * Finds files in a project.
 */

using System.Collections.Generic;
using System.IO;

namespace NexusEmbeddedEditor.Project
{
    public class ProjectStructure
    {
        private IProjectStructureInformation Information;
        private string ProjectDirectory;
        
        /*
         * Creates a project structure object.
         */
        public ProjectStructure(IProjectStructureInformation information,string projectDirectory)
        {
            this.Information = information;
            this.ProjectDirectory = projectDirectory;
        }
        
        /*
         * Returns the name of the project.
         */
        public string GetName()
        {
            return this.Information.GetName();
        }
        
        /*
         * Returns the directory of the project.
         */
        public string GetProjectDirectory()
        {
            return this.ProjectDirectory;
        }
        
        /*
         * Returns if a file name is valid for a script.
         */
        public bool IsScriptNameValid(string fileLocation,string scriptName)
        {
            // Remove the directory information from the file.
            fileLocation = Path.GetFileName(fileLocation.ToLower());
            scriptName = scriptName.ToLower();
            
            // Return if the script names match.
            if (fileLocation == scriptName)
            {
                return true;
            }
            
            // Return if start of the file before any extension is the name.
            if (fileLocation.Length > scriptName.Length && fileLocation.StartsWith(scriptName) && fileLocation.Substring(scriptName.Length, 1) == ".")
            {
                return true;
            }

            // Return false (not valid).
            return false;
        }
        
        /*
         * Return the possible files for a given name in a directory.
         */
        public HashSet<string> GetPossibleFileLocations(string directory, string scriptName)
        {
            // Return if the directory is a file or doesn't exist.
            if (File.Exists(directory) && this.IsScriptNameValid(directory, scriptName))
            {
                return new HashSet<string>() { directory };
            } else if (!Directory.Exists(directory))
            {
                return new HashSet<string>();
            }
            
            // Iterate through the directories and add the results.
            var validFiles = new HashSet<string>();
            foreach (var subFile in Directory.GetFiles(directory))
            {
                foreach (var entry in this.GetPossibleFileLocations(Path.Combine(directory,subFile),scriptName))
                {
                    validFiles.Add(entry);
                }
            }
            foreach (var subFile in Directory.GetDirectories(directory))
            {
                foreach (var entry in this.GetPossibleFileLocations(Path.Combine(directory,subFile),scriptName))
                {
                    validFiles.Add(entry);
                }
            }
            
            // Return the valid files.
            return validFiles;
        }
        
        /*
         * Returns the file location for a script.
         */
        public string GetFileLocation(string scriptName,string scriptSource)
        {
            scriptSource = scriptSource.Replace("\n","").Replace("\r","");
            
            // Get the possible files.
            var possibleFiles = new HashSet<string>();
            string firstEntry = null;
            foreach (var fileLocation in this.Information.GetSearchDirectories())
            {
                foreach (var entry in this.GetPossibleFileLocations(Path.Combine(this.ProjectDirectory,fileLocation),scriptName))
                {
                    possibleFiles.Add(entry);
                    if (firstEntry == null)
                    {
                        firstEntry = entry;
                    }
                }
            }
            
            // If more than 1 file exists, return the first file that matches the source.
            if (possibleFiles.Count > 1)
            {
                foreach (var fileLocation in possibleFiles)
                {
                    var fileSource = File.ReadAllText(fileLocation).Replace("\n","").Replace("\r","");
                    if (fileSource.StartsWith(scriptSource))
                    {
                        return fileLocation;
                    }
                }
            }

            // Return the first entry (will be null if none were found).
            return firstEntry;
        }
    }
}