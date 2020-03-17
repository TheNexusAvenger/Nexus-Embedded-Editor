/*
 * TheNexusAvenger
 *
 * Stores sessions and removes them when they are invalid.
 */

using System.Collections.Generic;

namespace NexusEmbeddedEditor.Server
{
    public class SessionStorage
    {
        private Dictionary<string,Session> Sessions;
        
        /*
         * Creates a session storage object.
         */
        public SessionStorage()
        {
            this.Sessions = new Dictionary<string,Session>();
        }
        
        /*
         * Gets a session.
         * Returns null if the session doesn't exist or is invalid.
         */
        public Session GetSession(string sessionId)
        {
            // Return null if the session doesn't exist.
            if (!this.Sessions.ContainsKey(sessionId))
            {
                return null;
            }
            
            // Remove the session and return null if the session is invalid.
            var session = this.Sessions[sessionId];
            if (!session.EditorWindow.Active)
            {
                this.CloseSession(sessionId);
                return null;
            }
            
            // Return the session.
            return session;
        }
        
        /*
         * Stores a session.
         */
        public void StoreSession(string sessionId,Session session)
        {
            this.Sessions[sessionId] = session;
        }
        
        /*
         * Closes a session.
         */
        public void CloseSession(string sessionId)
        {
            if (this.Sessions.ContainsKey(sessionId))
            {
                var session = this.Sessions[sessionId];
                session.EditorWindow.Close();
                session.StudioWindow.Window.Release();
                this.Sessions.Remove(sessionId);
            }
        }
    }
}