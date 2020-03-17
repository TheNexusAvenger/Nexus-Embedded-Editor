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
         * Returns if a file name is valid for a script.
         */
        public bool IsScriptNameValid(string fileLocation,string scriptName)
        {
            // Remove the directory information from the file.
            var fileName = Path.GetFileName(fileLocation.ToLower());
            scriptName = scriptName.ToLower();
            
            // Return if the script names match.
            if (fileName == scriptName)
            {
                return true;
            }
            
            // Return if start of the file before any extension is the name.
            if (fileName.Length > scriptName.Length && fileName.StartsWith(scriptName) && fileName.Substring(scriptName.Length, 1) == ".")
            {
                return true;
            }

            // Return false (not valid).
            return false;
        }
        
        /*
         * Return the possible files for a given name in a directory.
         */
        /*
        public HashSet<string> GetPossibleFileLocations(string directory,string scriptName,string parentName)
        {
            // Return if the directory is a file or doesn't exist.
            if (File.Exists(directory) && this.IsScriptNameValid(directory,scriptName,parentName))
            {
                return new HashSet<string>() { directory };
            } else if (!Directory.Exists(directory))
            {
                return new HashSet<string>();
            }
            
            // Iterate through the directories and add the results.
            var validFiles = new HashSet<string>();
            var parentDirectory = Directory.GetParent(directory).Name;
            foreach (var subFile in Directory.GetFiles(directory))
            {
                foreach (var entry in this.GetPossibleFileLocations(Path.Combine(directory,subFile),scriptName,parentDirectory))
                {
                    validFiles.Add(entry);
                }
            }
            foreach (var subFile in Directory.GetDirectories(directory))
            {
                foreach (var entry in this.GetPossibleFileLocations(Path.Combine(directory,subFile),scriptName,parentDirectory))
                {
                    validFiles.Add(entry);
                }
            }
            
            // Return the valid files.
            return validFiles;
        }
        */
        
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
            
            // Set up the file.
            if (File.Exists(fileLocation))
            {
                robloxFile.Location = fileLocation;
                if (name == Path.GetFileName(fileLocation))
                {
                    robloxFile.Name = name;
                }
                else
                {
                    robloxFile.Name = name + this.GetScriptExtension(fileLocation);
                }
            } else if (Directory.Exists(fileLocation))
            {
                // Add the init file (makes the folder a script).
                if (File.Exists(Path.Combine(fileLocation,"init.lua")))
                {
                    robloxFile.Name = name + ".lua";
                    robloxFile.Location = Path.Combine(fileLocation,"init.lua");
                } else if (File.Exists(Path.Combine(fileLocation,"init.server.lua")))
                {
                    robloxFile.Name = name + ".server.lua";
                    robloxFile.Location = Path.Combine(fileLocation,"init.server.lua");
                } else if (File.Exists(Path.Combine(fileLocation,"init.client.lua")))
                {
                    robloxFile.Name = name + ".client.lua";
                    robloxFile.Location = Path.Combine(fileLocation,"init.client.lua");
                }
                
                // Add the child scripts.
                foreach (var file in Directory.GetFiles(fileLocation))
                {
                    robloxFile.Files.Add(this.GetRobloxFiles(file,Path.GetFileName(file)));
                }
                foreach (var file in Directory.GetDirectories(fileLocation))
                {
                    robloxFile.Files.Add(this.GetRobloxFiles(file,Path.GetFileName(file)));
                }
            }
            
            // Return the Roblox file.
            return robloxFile;
        }
        
        /*
         * Returns the possible files for a name.
         */
        public HashSet<string> GetPossibleLocations(RobloxFile robloxFile,string scriptName)
        {
            var files = new HashSet<string>();
            
            // Add the file if it is valid.
            if (robloxFile.Name != null && this.IsScriptNameValid(robloxFile.Name,scriptName))
            {
                files.Add(robloxFile.Location);
            }
            
            // Add the subfiles.
            foreach (var subRobloxFile in robloxFile.Files)
            {
                foreach (var file in this.GetPossibleLocations(subRobloxFile,scriptName))
                {
                    files.Add(file);
                }
            }
            
            // Return the files.
            return files;
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
            foreach (var fileLocationData in this.Information.GetSearchDirectories())
            {
                foreach (var fileLocation in this.GetPossibleLocations(this.GetRobloxFiles(Path.Combine(this.ProjectDirectory,fileLocationData.Value),fileLocationData.Key),scriptName))
                {
                    possibleFiles.Add(fileLocation);
                    if (firstEntry == null)
                    {
                        firstEntry = fileLocation;
                    }
                }
            }
            /*
            foreach (var fileLocationData in this.Information.GetSearchDirectories())
            {
                var fileLocation = Path.Combine(this.ProjectDirectory,fileLocationData.Value);
                if (File.Exists(fileLocation))
                {
                    foreach (var entry in this.GetPossibleFileLocations(fileLocation,scriptName,fileLocationData.Key))
                    {
                        possibleFiles.Add(entry);
                        if (firstEntry == null)
                        {
                            firstEntry = entry;
                        }
                    }
                } else {
                    if (Directory.Exists(fileLocation))
                    {
                        foreach (var file in Directory.GetDirectories(fileLocation))
                        {
                            foreach (var entry in this.GetPossibleFileLocations(file,scriptName,fileLocationData.Key))
                            {
                                possibleFiles.Add(entry);
                                if (firstEntry == null)
                                {
                                    firstEntry = entry;
                                }
                            }
                        }
                        foreach (var file in Directory.GetFiles(fileLocation))
                        {
                            foreach (var entry in this.GetPossibleFileLocations(file,scriptName,fileLocationData.Key))
                            {
                                possibleFiles.Add(entry);
                                if (firstEntry == null)
                                {
                                    firstEntry = entry;
                                }
                            }
                        }
                    }
                }
            }
            */
            
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