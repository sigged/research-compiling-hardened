﻿using Microsoft.AspNetCore.Hosting;
using Sigged.CodeHost.Core.Dto;
using Sigged.CodeHost.Core.Logging;
using Sigged.CodeHost.Core.Worker;
using Sigged.CsC.NetCore.Web.Constants;
using Sigged.CsC.NetCore.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Sigged.CsC.NetCore.Web.Services
{
    public class RemoteCodeSessionManager 
    {
        protected IHostingEnvironment env;
        protected IWorkerService listener;
        protected IClientService clientService;
        protected List<RemoteCodeSession> sessions;

        public const int LISTENPORT = 2000;
        public const string WEBCONTAINERALIAS = "mainweb";

        public RemoteCodeSessionManager(IHostingEnvironment henv, IClientService clientservice)
        {
            env = henv;
            clientService = clientservice;
            listener = new WorkerTcpListener(IPAddress.Any, LISTENPORT);
            sessions = new List<RemoteCodeSession>();

            listener.WorkerConnected += Listener_WorkerConnected;
            listener.WorkerCompletedBuild += Listener_WorkerCompletedBuild;
            listener.WorkerExecutionStateChanged += Listener_WorkerExecutionState;

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
            
            var removeSessions = Sessions.Where(s => s.LastActivity.AddSeconds(SessionConstants.SessionIdleTimeout) <= DateTimeOffset.Now).ToList();
            Logger.LogLine($"CleanUpIdleSessions() - found {removeSessions.Count}/{Sessions.Count()} expired sessions");
            foreach (var session in removeSessions)
            {
                lock (session)
                {
                    Logger.LogLine($"Cleaning up idle session {session.SessionId}, last heartbeat @{session.LastActivity} expired at {session.LastActivity.AddSeconds(SessionConstants.SessionIdleTimeout)}");
                    ResetSessionWorker(session, WorkerResetReason.Expired);
                    sessions.Remove(session);
                }
            }
        }

        public void KillAllSessions()
        {

            var removeSessions = Sessions.ToList();
            Logger.LogLine($"KillAllSessions() - found {removeSessions.Count}/{Sessions.Count()} sessions to kill");
            foreach (var session in removeSessions)
            {
                lock (session)
                {
                    Logger.LogLine($"Cleaning up session {session.SessionId}");
                    ResetSessionWorker(session, WorkerResetReason.Expired);
                    sessions.Remove(session);
                }
            }
        }

        public RemoteCodeSession CreateSession(string uniqueSessionId)
        {
            string sessionid = uniqueSessionId;
            
            var session = new RemoteCodeSession()
            {
                SessionId = sessionid
            };
            lock (this)
            {
                session.Heartbeat();
                sessions.Add(session);
            }
            return session;
        }

        public RemoteCodeSession GetSession(string sessionid)
        {
            var session = sessions.FirstOrDefault(s => s.SessionId == sessionid);
            session?.Heartbeat();
            return session;
        }

        protected async Task CreateWorkerProcess(RemoteCodeSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            else
            {
                await Task.Run(() => {
                    try
                    {
                        //string workerExePath = Path.Combine(env.ContentRootPath, "_workerProcess", "worker", "Sigged.CodeHost.Worker.dll");
                        //Logger.LogLine($"Starting process at {workerExePath}");
                        //session.WorkerProcess = new NativeWorkerProcess(workerExePath);

                        
                        session.WorkerProcess = new DockerWorkerProcess();
                        session.WorkerProcess.Start(WEBCONTAINERALIAS, LISTENPORT, session.SessionId);
                    }
                    catch (IOException ioex)
                    {
                        Logger.LogLine(ioex.Message);
                        throw;
                    }
                    catch(Exception ex)
                    {
                        Logger.LogLine(ex.Message);
                        throw;
                    }
                    
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
            
            //reuse existing worker process if still running and connected
            if(session.WorkerProcess != null && session.WorkerProcess.HasExited() == false)
            {
                if(session.WorkerClient?.Connected == true)
                {
                    Logger.LogLine("Recycling worker process for new build request");
                    listener.SendWorkerMessage(session.WorkerClient, MessageType.ServerBuildRequest, session.LastBuildRequest);
                    return;
                }
                else
                {
                    //worker process has quit or disconnected. Reset references so it can be recreated.
                    Logger.LogLine("Resetting stopped/disconnected worker process for new build request");
                    ResetSessionWorker(session, WorkerResetReason.WorkerStopped);
                }
            }
            //no worker process, create from scratch so it can connect
            Logger.LogLine("Creating new worker process for new build request");
            await CreateWorkerProcess(session);
        }

        /// <summary>
        /// Cancels any active operations for a given session
        /// </summary>
        /// <returns></returns>
        public void CancelSessionActions(string sessionid)
        {
            var session = GetSession(sessionid);
            if (session != null)
            {
                ResetSessionWorker(session, WorkerResetReason.UserCancelled);
            }

        }

        protected void ResetSessionWorker(RemoteCodeSession session, WorkerResetReason reason)
        {
            Logger.LogLine($"Worker Reset: {session.SessionId} because {reason}");
            session.WorkerClient?.Close();
            Logger.LogLine($"Worker Reset: closed worker socket of {session.SessionId}");
            try
            {
                if (session.WorkerProcess?.HasExited() == false)
                {
                    session.WorkerProcess?.Kill();
                    Logger.LogLine($"Worker Reset: killed worker process of {session.SessionId}");
                    //notify client of worker destruction
                    if(reason == WorkerResetReason.Expired &&
                         //don't notify if worker is no longer executing user code
                         session.LastAppState != RemoteAppState.NotRunning &&
                         session.LastAppState != RemoteAppState.Ended &&
                         session.LastAppState != RemoteAppState.Crashed
                    )
                    {
                        clientService.SendExecutionState(session.SessionId, new ExecutionStateDto
                        {
                            SessionId = session.SessionId,
                            State = RemoteAppState.Crashed,
                            Exception = ExceptionDto.FromException(new TimeoutException(
                                $"Your application takes longer than {SessionConstants.SessionIdleTimeout} seconds to execute and has been terminated."))
                        });
                    }
                }
            }
            finally
            {
                session.WorkerClient?.Dispose();
                session.WorkerProcess?.Dispose();
                session.WorkerProcess = null;
                session.WorkerClient = null;
            }
            
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
                Logger.LogLine(ex.Message);
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

        protected void Listener_WorkerExecutionState(TcpClient workerClient, ExecutionStateDto state)
        {
            var session = GetSession(state.SessionId);
            if(session != null)
            {
                session.LastAppState = state.State;
                if(session.LastAppState == RemoteAppState.Running)
                    session.Heartbeat(); //idle timer reset on start of user code execution
            }

            clientService.SendExecutionState(state.SessionId, state);
        }

    }
}
