/*
 * TheNexusAvenger
 *
 * Client request handler for opening a script.
 */

using System.Text;
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;

namespace NexusEmbeddedEditor.Server
{
    public class OpenScriptRequestHandler : IClientRequestHandler
    {
        private SessionStorage Sessions;
        
        /*
         * Creates a request handler object.
         */
        public OpenScriptRequestHandler(SessionStorage sessions)
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
            
            // Open the script and return a success response.
            // TODO: Enable fetching the source.
            session.StudioWindow.OpenScript(request.GetURL().GetParameter("script"),Encoding.UTF8.GetString(request.GetBody()));
            return HttpResponse.CreateSuccessResponse("Success.");
        }
    }
}