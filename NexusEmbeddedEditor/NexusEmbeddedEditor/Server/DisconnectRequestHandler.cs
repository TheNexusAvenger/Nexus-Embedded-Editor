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
        private SessionStorage Sessions;
        
        /*
         * Creates a request handler object.
         */
        public DisconnectRequestHandler(SessionStorage sessions)
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
            var sessionId = request.GetURL().GetParameter("session");
            var session = this.Sessions.GetSession(sessionId);
            if (session == null)
            {
                return HttpResponse.CreateBadRequestResponse("Session not found.");
            }
            
            // Close the session and return a success response.
            this.Sessions.CloseSession(sessionId);
            return HttpResponse.CreateSuccessResponse("Success.");
        }
    }
}