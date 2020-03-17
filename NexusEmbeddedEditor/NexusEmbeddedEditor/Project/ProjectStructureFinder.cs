/*
 * TheNexusAvenger
 *
 * Attempts to find 
 */

using System.Collections.Generic;
using System.IO;

namespace NexusEmbeddedEditor.Project
{
    public class ProjectStructureFinder
    {
        /*
         * Attempts to find a valid project structure for the given
         * directory or a parent directory. Returns null if no valid
         * project structure was found.
         */
        public static ProjectStructure FindProjectStructure(string directory,string[] args)
        {
            // Create the projects.
            var structures = new List<IProjectStructureInformation>()
            {
                new RojoLegacy(directory),
                new Rojo(directory,args),
            };
            
            // Iterate through the projects.
            foreach (var structureInformation in structures)
            {
                if (structureInformation.IsProjectStructureValid())
                {
                    return new ProjectStructure(structureInformation,directory);
                }
            }
            
            // Return the parent directory.
            var parentDirectory = Directory.GetParent(directory);
            if (parentDirectory != null)
            {
                return FindProjectStructure(parentDirectory.FullName,args);
            }
            
            // Return null (no parent directory).
            return null;
        }
    }
}