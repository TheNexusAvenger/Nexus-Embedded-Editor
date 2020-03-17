/*
 * TheNexusAvenger
 *
 * Client request handler for attaching an editor to Roblox Studio.
 */

using System.Collections.Generic;
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;

namespace NexusEmbeddedEditor.Server
{
    public class AttachRequestHandler : IClientRequestHandler
    {
        private Dictionary<string,Session> Sessions;
        
        /*
         * Creates a request handler object.
         */
        public AttachRequestHandler(Dictionary<string,Session> sessions)
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
            
            // Set the editor as attached and return a success response.
            this.Sessions[session].EditorWindow.Attached = true;
            return HttpResponse.CreateSuccessResponse("Success.");
        }
    }
}