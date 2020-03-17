/*
 * TheNexusAvenger
 *
 * Interface for project structure information.
 */

using System.Collections.Generic;

namespace NexusEmbeddedEditor.Project
{
    public interface IProjectStructureInformation
    {
        /*
         * Returns the name of the project structure.
         */
        string GetName();
        
        /*
         * Returns if a structure is valid for the directory.
         */
        bool IsProjectStructureValid();
        
        /*
         * Returns the files and directories to search through for a file.
         */
        List<string> GetSearchDirectories();
    }
}