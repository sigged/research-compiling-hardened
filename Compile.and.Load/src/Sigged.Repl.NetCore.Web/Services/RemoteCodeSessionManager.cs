using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Emit;
using ProtoBuf;
using Sigged.CodeHost.Core.Dto;
using Sigged.Compiling.Core;
using Sigged.Repl.NetCore.Web.Extensions;
using Sigged.Repl.NetCore.Web.Models;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class RemoteCodeSessionManager 
    {
        protected IHostingEnvironment env;
        protected IWorkerService listener;
        protected IClientService clientService;
        protected List<RemoteCodeSession> sessions;

        public RemoteCodeSessionManager(IHostingEnvironment henv, IClientService clientservice)
        {
            env = henv;
            clientService = clientservice;
            listener = new WorkerTcpListener(IPAddress.Any, 2000);
            sessions = new List<RemoteCodeSession>();

            listener.WorkerConnected += Listener_WorkerConnected;
            listener.WorkerCompletedBuild += Listener_WorkerCompletedBuild;
            listener.WorkerExecutionStateChanged += Listener_WorderExecutionState;

            if (!listener.IsListening)
            {
                bool ok = listener.StartListening();
                if (!ok)
                    throw new ApplicationException($"Listener failed to start");
            }
            clientService.Connect();
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

            //todo: remove after debug
            //sessionid = "MYSESSION";

            var session = new RemoteCodeSession()
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

        protected async Task CreateWorkerProcess(RemoteCodeSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (session.WorkerProcess != null && session.WorkerProcess.HasExited == false)
            {
                throw new InvalidOperationException($"Session {session.SessionId} still has a worker process running.");
            }
            else
            {
                await Task.Run(() => {
                    string workerExePath = Path.Combine(env.ContentRootPath, "_workerProcess", "worker", "Sigged.CodeHost.Worker.dll");

                    session.WorkerProcess = new Process();
                    session.WorkerProcess.EnableRaisingEvents = true;
                    session.WorkerProcess.Exited += (object sender, EventArgs e) => {
                        session.WorkerProcess = null;
                        session.WorkerClient = null;
                    };

                    session.WorkerProcess.StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"{workerExePath} localhost 2000 {session.SessionId}",
                        UseShellExecute = true
                    };

                    session.WorkerProcess.Start();
                });
            }
        }
        
        /// <summary>
        /// Processes user's build request by creating a worker process and forwarding request
        /// </summary>
        /// <returns></returns>
        public async Task ProcessUserBuildRequest(BuildRequestDto buildRequest)
        {
            var session = GetSession(buildRequest.SessionId);
            if (session == null)
            {
                session = CreateSession(buildRequest.SessionId);
                buildRequest.SessionId = session.SessionId;
            }

            //set build request so it can be picked up after worker connection
            session.LastBuildRequest = buildRequest;
            
            await CreateWorkerProcess(session);
        }

        /// <summary>
        /// Forwards user's console input to worker process
        /// </summary>
        /// <returns></returns>
        internal void ForwardConsoleInput(RemoteInputDto remoteInput)
        {
            try
            {
                var session = GetSession(remoteInput.SessionId);
                if (session == null)
                    throw new InvalidOperationException("Can't forward input: Session doesn't exist");
                if (session.WorkerClient == null)
                    throw new InvalidOperationException("Can't forward input: Session has no identified worker");
                if (!session.WorkerClient.Connected)
                    throw new InvalidOperationException("Can't forward input: Disconnected from session's worker");

                listener.SendWorkerMessage(session.WorkerClient, MessageType.ServerRemoteInput, remoteInput);
            }
            catch(Exception ex)
            {
                Debug.Fail(ex.Message);
            }
            
        }

        protected void Listener_WorkerConnected(TcpClient workerClient, string sessionId)
        {
            var session = GetSession(sessionId);
            if (session == null)
            {
                //no session found for this client, kill connection
                workerClient.Close();
            }
            else
            {
                session.WorkerClient = workerClient;
                if (session.LastBuildRequest != null)
                {
                    listener.SendWorkerMessage(session.WorkerClient, MessageType.ServerBuildRequest, session.LastBuildRequest);
                }
                else
                {
                    throw new InvalidOperationException($"Session {sessionId} has no LastBuildRequest set");
                }
            }
        }

        protected void Listener_WorkerCompletedBuild(TcpClient workerClient, BuildResultDto result)
        {
            clientService.SendBuildResult(result.SessionId, result);
        }

        protected void Listener_WorderExecutionState(TcpClient workerClient, ExecutionStateDto state)
        {
            clientService.SendExecutionState(state.SessionId, state);
        }

    }
}
