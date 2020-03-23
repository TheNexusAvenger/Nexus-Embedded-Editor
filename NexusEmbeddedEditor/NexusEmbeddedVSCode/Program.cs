/*
 * TheNexusAvenger
 *
 * Runs the program for Microsoft Visual Studio Code.
 */

using System;
using System.IO;

namespace NexusEmbeddedVSCode
{
    internal class Program
    {
        // The port to run the server.
        public const int SERVER_PORT = 35871;
        
        /*
         * Runs the program.
         */
        public static void Main(string[] args)
        {
            // Display a message if Microsoft Visual Studio Code is not detected.
            var executableLocation = VisualStudioCodeWindow.GetExecutableLocation();
            if (executableLocation == null || !File.Exists(executableLocation))
            {
                Console.WriteLine("Microsoft Visual Studio Code was not found at " + Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"),"Programs","Microsoft VS Code","Code.exe") + " or in the system's PATH environment.");
                return;
            }
            
            // Start the program.
            NexusEmbeddedEditor.Program.Main(args,SERVER_PORT,(structure) =>
            {
                return VisualStudioCodeWindow.OpenWindow(structure);
            });
        }
    }
}