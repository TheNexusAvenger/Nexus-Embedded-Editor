/*
 * TheNexusAvenger
 *
 * Client request handler for reading a script.
 */

using System.Text;
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;

namespace NexusEmbeddedEditor.Server
{
    public class ReadScriptRequestHandler : IClientRequestHandler
    {
        private SessionStorage Sessions;
        
        /*
         * Creates a request handler object.
         */
        public ReadScriptRequestHandler(SessionStorage sessions)
        {
            this.Sessions = sessions;
        }
        
        /*
         * Handles a client request.
         */
        public HttpResponse GetResponseData(HttpRequest request)
        {
            // Return an error if the session or script path is not defined.
            if (!request.GetURL().ParameterExists("session"))
            {
                return HttpResponse.CreateBadRequestResponse("Session parameter not defined.");
            }
            if (!request.GetURL().ParameterExists("script"))
            {
                return HttpResponse.CreateBadRequestResponse("script parameter not defined.");
            }
            
            // Return an error if the session doesn't exist.
            var sessionId = request.GetURL().GetParameter("session");
            var session = this.Sessions.GetSession(sessionId);
            if (session == null)
            {
                return HttpResponse.CreateBadRequestResponse("Session not found.");
            }
            
            // Return the source of the script.
            return HttpResponse.CreateSuccessResponse(session.StudioWindow.ReadScript(request.GetURL().GetParameter("script")));
        }
    }
}