/*
 * TheNexusAvenger
 *
 * Client request handler for fetching session information.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;

namespace NexusEmbeddedEditor.Server
{
    public class SessionResponse
    {
        public string session;
        public bool attached;
    }
    
    public class SessionRequestHandler : IClientRequestHandler
    {
        private SessionStorage Sessions;
        
        /*
         * Creates a request handler object.
         */
        public SessionRequestHandler(SessionStorage sessions)
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
            
            // Create and return the response.
            var response = new SessionResponse();
            response.session = sessionId;
            response.attached = session.EditorWindow.Attached;
            return HttpResponse.CreateSuccessResponse(JsonConvert.SerializeObject(response));
        }
    }
}