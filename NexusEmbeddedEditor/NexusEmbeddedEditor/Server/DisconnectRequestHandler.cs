/*
 * TheNexusAvenger
 *
 * Client request handler for disconnecting from Roblox Studio.
 */

using System.Collections.Generic;
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;

namespace NexusEmbeddedEditor.Server
{
    public class DisconnectRequestHandler : IClientRequestHandler
    {
        private Dictionary<string,Session> Sessions;
        
        /*
         * Creates a request handler object.
         */
        public DisconnectRequestHandler(Dictionary<string,Session> sessions)
        {
            this.Sessions = sessions;
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
            
            // Return an error if the session doesn't exist.
            var session = request.GetURL().GetParameter("session");
            if (!this.Sessions.ContainsKey(session))
            {
                return HttpResponse.CreateBadRequestResponse("Session not found.");
            }
            
            // Close the session and return a success response.
            var activeSession = this.Sessions[session];
            activeSession.EditorWindow.Close();
            activeSession.StudioWindow.Window.Release();
            this.Sessions.Remove(session);
            return HttpResponse.CreateSuccessResponse("Success.");
        }
    }
}