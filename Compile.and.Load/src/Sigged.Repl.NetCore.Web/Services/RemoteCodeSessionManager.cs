using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Emit;
using Sigged.Compiling.Core;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class RemoteCodeSessionManager 
    {
        protected List<RemoteCodeSession> sessions;
        protected Compiler compiler;
        protected IHostingEnvironment env;
        protected IRemoteExecutionCallback remoteExecutionCallback;
            
        public RemoteCodeSessionManager(IHostingEnvironment henv, IRemoteExecutionCallback executionCaller)
        {
            env = henv;
            remoteExecutionCallback = executionCaller;
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

        public RemoteCodeSession CreateSession(string uniqueSessionId)
        {
            //string sessionid = Guid.NewGuid().ToString();
            string sessionid = uniqueSessionId;
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
            try
            {
                session.IsBuilding = true;
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
            finally
            {
                session.IsBuilding = false;
            }
        }

        public void RunLastCompilation(string sessionid)
        {
            var session = GetSession(sessionid);
            if (session == null)
                throw new InvalidOperationException($"Can't build for non-existing session {sessionid}");
            if (session.LastAssembly == null || session.LastResult == null)
                throw new InvalidOperationException($"Code for session {sessionid} has not been built yet");

            session.IsRunning = true;
            //cancel any previous threads, just to be absolutely sure
            if (session.ExecutionThread?.IsAlive == true)
            {
                Trace.TraceWarning($"Warning! Session {session.SessionId} runs new thread while old thread still running. Disposing old thread...");
                try
                {
                    session.ExecutionThread.Abort();
                }
                catch (ThreadAbortException tae)
                {
                    Trace.TraceWarning($"Warning! Disposed of old thread for session {session.SessionId} ");
                }
            }

            session.ExecutionThread = new Thread(new ParameterizedThreadStart((object sessionObj) =>
            {
                var execSession = (RemoteCodeSession)sessionObj;
                execSession.IsRunning = true;

                var assemly = Assembly.Load(execSession.LastAssembly);
                var type = assemly.GetType("Test.Program");
                //var test = type.FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.Public, null, null);
                try
                {
                    remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
                    {
                        State = RemoteAppState.Running
                    });

                    type.InvokeMember("Main",
                                        BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public,
                                        null, null,
                                        new object[] { new string[] { } });

                    remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
                    {
                        State = RemoteAppState.Ended
                    });
                }
                catch (Exception ex)
                {
                    remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
                    {
                        State = RemoteAppState.Crashed
                    });
                }
                finally
                {
                    execSession.IsRunning = false;
                }
            }));
            session.ExecutionThread.Start(session);
            
        }
        
    }
}
