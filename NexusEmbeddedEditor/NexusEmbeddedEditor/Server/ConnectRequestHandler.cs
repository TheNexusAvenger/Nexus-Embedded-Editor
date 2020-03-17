/*
 * TheNexusAvenger
 *
 * Client request handler for connecting an editor to Roblox Studio.
 */

using System;
using System.Collections.Generic;
using System.Windows.Automation;
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;
using NexusEmbeddedEditor.Project;
using NexusEmbeddedEditor.Window;

namespace NexusEmbeddedEditor.Server
{
    public class ConnectRequestHandler : IClientRequestHandler
    {
        private Dictionary<string,Session> Sessions;
        private ProjectStructure Structure;
        private Func<ProjectStructure,EditorWindow> InitializeWindowFunction;
        
        /*
         * Creates a request handler object.
         */
        public ConnectRequestHandler(Dictionary<string,Session> sessions,ProjectStructure structure,Func<ProjectStructure,EditorWindow> initializeWindowFunction)
        {
            this.Sessions = sessions;
            this.Structure = structure;
            this.InitializeWindowFunction = initializeWindowFunction;
        }
        
        /*
         * Handles a client request.
         */
        public HttpResponse GetResponseData(HttpRequest request)
        {
            // Return an error if the session is not defined.
            if (!request.GetURL().ParameterExists("session"))
            {
                return HttpResponse.CreateBadRequestResponse("Session parameter not defined.");
            }
            
            // Return an error if the session already exists.
            var session = request.GetURL().GetParameter("session");
            if (this.Sessions.ContainsKey(session))
            {
                return HttpResponse.CreateBadRequestResponse("Session already exists.");
            }
            
            // Initialize the windows.
            RobloxStudioWindow studioWindow = null;
            try
            {
                studioWindow = RobloxStudioWindow.GetWindow(session,this.Structure);
            }
            catch (ElementNotAvailableException)
            {
                return HttpResponse.CreateBadRequestResponse("Session not detected.");
            }
            var editorWindow = this.InitializeWindowFunction(this.Structure);
            studioWindow.SetExternalEditor(editorWindow);
            
            // Add the session and return a success response.
            var sessionObject = new Session();
            sessionObject.StudioWindow = studioWindow;
            sessionObject.EditorWindow = editorWindow;
            this.Sessions[session] = sessionObject;
            return HttpResponse.CreateSuccessResponse("Success.");
        }
    }
}