using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis.Emit;
using Sigged.Compiling.Core;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class RemoteCodeSessionManager : IRemoteCodeSessionManager
    {
        protected List<RemoteCodeSession> sessions;
        protected Compiler compiler;
        protected IHostingEnvironment env;

        public RemoteCodeSessionManager(IHostingEnvironment henv)
        {
            env = henv;
            string netstandardRefsDirectory = Path.Combine(env.ContentRootPath, "_libs", "netstandard2.0");
            compiler = new Compiler(netstandardRefsDirectory);
            sessions = new List<RemoteCodeSession>();
        }

        public IEnumerable<RemoteCodeSession> Sessions {
            get
            {
                return sessions;
            }
        }

        public void CleanupIdleSessions()
        {
            var removeSessions = Sessions.Where(s => s.LastActivity < DateTimeOffset.Now.AddMinutes(3)).ToList();
            foreach (var session in removeSessions)
            {
                lock (session)
                {
                    sessions.Remove(session);
                }
            }
        }

        public RemoteCodeSession CreateSession()
        {
            string sessionid = Guid.NewGuid().ToString();
            var session = new RemoteCodeSession
            {
                SessionId = sessionid,
                LastActivity = DateTimeOffset.Now,
                LastAssembly = null,
                LastResult = null,
            };
            lock (this)
            {
                sessions.Add(session);
            }
            return session;
        }

        public RemoteCodeSession GetSession(string sessionid)
        {
            return sessions.FirstOrDefault(s => s.SessionId == sessionid);
        }

        public async Task<EmitResult> Compile(string sessionid, string code)
        {
            var session = GetSession(sessionid);
            if (session == null)
            {
                throw new InvalidOperationException($"Can't build for non-existing session {sessionid}");
            }

            EmitResult results = null;
            byte[] assembly = null;
            using (var stream = new MemoryStream())
            {
                results = await compiler.Compile(code, sessionid.ToString(), stream);
                assembly = stream.ToArray();
            }
            lock (session)
            {
                session.LastResult = results;
                session.LastAssembly = assembly;
                session.LastActivity = DateTimeOffset.Now;
            }
            return results;
        }

        public async Task<bool> RunLastCompilation(string sessionid)
        {
            var session = GetSession(sessionid);
            if (session == null || session.LastAssembly == null)
            {
                throw new InvalidOperationException($"Can't run for non-existing session {sessionid}");
            }
            else
            {
                return true;
            }
        }
    }
}
