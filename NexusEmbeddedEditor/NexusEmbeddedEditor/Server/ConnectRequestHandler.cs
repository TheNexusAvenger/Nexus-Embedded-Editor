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
        private SessionStorage Sessions;
        private ProjectStructure Structure;
        private Func<ProjectStructure,EditorWindow> InitializeWindowFunction;
        
        /*
         * Creates a request handler object.
         */
        public ConnectRequestHandler(SessionStorage sessions,ProjectStructure structure,Func<ProjectStructure,EditorWindow> initializeWindowFunction)
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
            var sessionId = request.GetURL().GetParameter("session");
            var session = this.Sessions.GetSession(sessionId);
            if (session != null)
            {
                return HttpResponse.CreateBadRequestResponse("Session already exists.");
            }
            
            // Initialize the windows.
            RobloxStudioWindow studioWindow = null;
            try
            {
                studioWindow = RobloxStudioWindow.GetWindow(sessionId,this.Structure);
            }
            catch (ElementNotAvailableException)
            {
                return HttpResponse.CreateBadRequestResponse("Session not detected.");
            }
            var editorWindow = this.InitializeWindowFunction(this.Structure);
            studioWindow.SetExternalEditor(editorWindow);
            
            // Add the session and return a success response.
            session = new Session();
            session.StudioWindow = studioWindow;
            session.EditorWindow = editorWindow;
            this.Sessions.StoreSession(sessionId,session);
            return HttpResponse.CreateSuccessResponse("Success.");
        }
    }
}