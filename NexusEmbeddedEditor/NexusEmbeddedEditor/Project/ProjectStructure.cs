/*
 * TheNexusAvenger
 * 
 * Finds files in a project.
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace NexusEmbeddedEditor.Project
{
    public class RobloxFile
    {
        public string Name;
        public string Location;
        public List<RobloxFile> Files = new List<RobloxFile>();
        
        /*
         * Returns the file location for the instance path.
         */
        public string GetFileLocation(string path)
        {
            // Return if the path matches.
            if (path == this.Name)
            {
                return this.Location;
            }
            
            // Return null if the path does not start with the name.
            if (!path.StartsWith(this.Name))
            {
                return null;
            }
            
            // Get the remainder and return if a child matches.
            if (path.Length + 1 > this.Name.Length)
            {
                var remainder = path.Substring(this.Name.Length + 1);
                foreach (var child in this.Files)
                {
                    var childResult = child.GetFileLocation(remainder);
                    if (childResult != null)
                    {
                        return childResult;
                    }
                }
            }

            // Return null (not found).
            return null;
        }
    }
    
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
         * Returns the file extension for a path.
         */
        public string GetScriptExtension(string path)
        {
            // Return a server or client script.
            if (path.ToLower().EndsWith(".server.lua"))
            {
                return ".server.lua";
            } else if (path.ToLower().EndsWith(".client.lua"))
            {
                return ".client.lua";
            }
            
            // Return .lua.
            return ".lua";
        }
        
        /*
         * Gets the Roblox files from a file or directory.
         */
        public RobloxFile GetRobloxFiles(string fileLocation,string name)
        {
            // Create the file.
            var robloxFile = new RobloxFile();
            robloxFile.Name = name;
            
            // Set up the file.
            if (File.Exists(fileLocation))
            {
                robloxFile.Location = fileLocation;
                robloxFile.Name = name;
            } else if (Directory.Exists(fileLocation))
            {
                // Add the init file (makes the folder a script).
                if (File.Exists(Path.Combine(fileLocation,"init.lua")))
                {
                    robloxFile.Name = name;
                    robloxFile.Location = Path.Combine(fileLocation,"init.lua");
                } else if (File.Exists(Path.Combine(fileLocation,"init.server.lua")))
                {
                    robloxFile.Name = name;
                    robloxFile.Location = Path.Combine(fileLocation,"init.server.lua");
                } else if (File.Exists(Path.Combine(fileLocation,"init.client.lua")))
                {
                    robloxFile.Name = name;
                    robloxFile.Location = Path.Combine(fileLocation,"init.client.lua");
                }
                
                // Add the child scripts.
                foreach (var file in Directory.GetFiles(fileLocation))
                {
                    var subName = Path.GetFileName(file);
                    if (subName.Contains("."))
                    {
                        subName = subName.Substring(0,subName.IndexOf(".",StringComparison.Ordinal));
                    }
                    robloxFile.Files.Add(this.GetRobloxFiles(file,subName));
                }
                foreach (var file in Directory.GetDirectories(fileLocation))
                {
                    var subName = Path.GetFileName(file);
                    if (subName.Contains("."))
                    {
                        subName = subName.Substring(0,subName.IndexOf(".",StringComparison.Ordinal));
                    }
                    robloxFile.Files.Add(this.GetRobloxFiles(file,subName));
                }
            }
            
            // Return the Roblox file.
            return robloxFile;
        }

        /*
         * Returns the file location for a script.
         */
        public string GetFileLocation(string scriptPath)
        {
            // Iterate through the files.
            foreach (var fileLocationData in this.Information.GetSearchDirectories())
            {
                if (scriptPath.StartsWith(fileLocationData.Key))
                {
                    // Return the file if it exists.
                    var robloxFiles = this.GetRobloxFiles(Path.Combine(this.ProjectDirectory,fileLocationData.Value),fileLocationData.Key);
                    var fileLocation = robloxFiles.GetFileLocation(scriptPath);
                    if (fileLocation != null)
                    {
                        return fileLocation;
                    }
                }
            }

            // Return null (not found).
            return null;
        }
    }
}