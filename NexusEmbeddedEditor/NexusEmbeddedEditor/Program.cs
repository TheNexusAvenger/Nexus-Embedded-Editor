/*
 * TheNexusAvenger
 *
 * Runs the Nexus Embedded Editor program.
 * Can only be started externally to add information about the editor.
 */

using System;
using System.IO;
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Server;
using NexusEmbeddedEditor.Project;
using NexusEmbeddedEditor.Project.Information;
using NexusEmbeddedEditor.Server;
using NexusEmbeddedEditor.Window;

namespace NexusEmbeddedEditor
{
    public class Program
    {
        /*
         * Runs the program.
         */
        public static void Main(string[] args,int serverPort,Func<ProjectStructure,EditorWindow> initializeWindowFunction)
        {
            // Get the project structure.
            var projectStructure = ProjectStructureFinder.FindProjectStructure(Directory.GetCurrentDirectory(),args);
            if (projectStructure == null)
            {
                Console.WriteLine("No project structure was found in the directory or parent directories.");
                projectStructure = new ProjectStructure(new NullStructure(Directory.GetCurrentDirectory()),Directory.GetCurrentDirectory());
            }
            
            // Create the request handler.
            var requestHandler = new RequestHandler();
            var sessions = new SessionStorage();
            requestHandler.RegisterHandler("POST","/connect",new ConnectRequestHandler(sessions,projectStructure,initializeWindowFunction));
            requestHandler.RegisterHandler("POST","/attach",new AttachRequestHandler(sessions));
            requestHandler.RegisterHandler("POST","/detach",new DetachRequestHandler(sessions));
            requestHandler.RegisterHandler("POST","/disconnect",new DisconnectRequestHandler(sessions));
            requestHandler.RegisterHandler("POST","/openscript",new OpenScriptRequestHandler(sessions));
            requestHandler.RegisterHandler("GET","/session",new SessionRequestHandler(sessions));
            requestHandler.RegisterHandler("GET","/readscript",new ReadScriptRequestHandler(sessions));
            
            // Output the starter information.
            Console.WriteLine("Serving on port " + serverPort);
            Console.WriteLine("\tProject type: " + projectStructure.GetName());
            Console.WriteLine("\tProject directory: " + projectStructure.GetProjectDirectory());
            
            // Create and start the web server.
            var server = new HttpServer(serverPort,requestHandler);
            server.Start();
        }
    }
}