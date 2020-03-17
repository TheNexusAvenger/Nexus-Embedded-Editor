/*
 * TheNexusAvenger
 *
 * Runs the program for Microsoft Visual Studio Code.
 */

using System;
using System.IO;

namespace NexusEmbeddedMSVC
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
            if (!File.Exists(VisualStudioCodeWindow.GetExecutableLocation()))
            {
                Console.WriteLine("Microsoft Visual Studio Code was not found at " + VisualStudioCodeWindow.GetExecutableLocation());
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