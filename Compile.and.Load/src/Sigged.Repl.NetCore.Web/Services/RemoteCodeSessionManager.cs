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
using Sigged.Repl.NetCore.Web.Extensions;
using Sigged.Repl.NetCore.Web.Models;

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
            var session = new RemoteCodeSession(remoteExecutionCallback)
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
                    results = await compiler.Compile(code, sessionid.ToString(), stream, outputKind: Microsoft.CodeAnalysis.OutputKind.ConsoleApplication);
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

        //public void RunLastCompilation_PROCESS(string sessionid)
        //{
        //    var session = GetSession(sessionid);
        //    if (session == null)
        //        throw new InvalidOperationException($"Can't build for non-existing session {sessionid}");
        //    if (session.LastAssembly == null || session.LastResult == null)
        //        throw new InvalidOperationException($"Code for session {sessionid} has not been built yet");

        //    session.IsRunning = true;
        //    //cancel any previous threads, just to be absolutely sure
        //    if (session.Process?.HasExited == false)
        //    {
        //        Trace.TraceWarning($"Warning! Session {session.SessionId} started a new process while old process still running. Disposing old process...");
        //        session.Process.Kill();
        //    }

        //    session.IsRunning = true;

        //    string assemblyDir = Path.Combine(env.ContentRootPath, "_assemblytmp");
        //    string assemblyFilename = Path.Combine(assemblyDir, Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
        //    string assemblyConfigFileName = assemblyFilename + ".runtimeconfig.json";
        //    assemblyFilename += ".dll";

        //    try
        //    {
        //        //write assembly file and copy runtimeconfig
        //        using (var fs = new FileStream(assemblyFilename, FileMode.Create, FileAccess.Write))
        //        {
        //            fs.Write(session.LastAssembly, 0, session.LastAssembly.Length);
        //        }
        //        File.Copy(Path.Combine(assemblyDir, "templates", "_template_.runtimeconfig.json"), assemblyConfigFileName);

        //        //notify client of running state
        //        remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
        //        {
        //            State = RemoteAppState.Running
        //        });

        //        //start process
        //        Process process = new Process();
        //        process.EnableRaisingEvents = true;
        //        process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => { process_OutputDataReceived(session, e); };
        //        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => { process_ErrorDataReceived(session, e); }; ;
        //        process.Exited += (object sender, EventArgs e) => { process_Exited(session, e); }; ;

        //        process.StartInfo = new ProcessStartInfo
        //        {
        //            FileName = "dotnet",
        //            Arguments = assemblyFilename,
        //            UseShellExecute = false,
        //            RedirectStandardError = true,
        //            RedirectStandardOutput = true
        //        };

        //        process.Start();
        //        process.BeginOutputReadLine();
        //        process.BeginErrorReadLine();
        //    }
        //    catch (Exception ex)
        //    {
        //        remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
        //        {
        //            State = RemoteAppState.Crashed,
        //            Exception = ExceptionDescriptor.FromException(ex)
        //        });
        //        session.IsRunning = false;
        //        try
        //        {
        //            File.Delete(assemblyFilename);
        //            File.Delete(assemblyConfigFileName);
        //        }
        //        catch (Exception fileDelEx)
        //        {
        //            Console.WriteLine($"Error while deleting generated assembly file: {ex.Message}");
        //        }
        //    }
        //    finally
        //    {


        //    }

        //}

        //private void process_Exited(object sender, EventArgs e)
        //{
        //    var session = (RemoteCodeSession)sender;
        //    remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
        //    {
        //        State = RemoteAppState.Ended,
        //    });
        //    session.IsRunning = false;
        //}

        //private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data == null) return;
        //    var session = (RemoteCodeSession)sender;
        //    remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
        //    {
        //        State = RemoteAppState.Crashed,
        //        Exception = ExceptionDescriptor.FromException(new Exception(e.Data))
        //    });
        //}

        //private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data == null) return;
        //    var session = (RemoteCodeSession)sender;
        //    lock (session)
        //    {
        //        remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
        //        {
        //            State = RemoteAppState.WriteOutput,
        //            Output = e.Data
        //        });
        //    }
        //}

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
                    //session.ExecutionThread.Abort();
                }
                catch (Exception tae)
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

                    //redirect console
                    Console.SetOut(session.consoleOutputRedirector);
                    Console.SetIn(session.consoleInputRedirector);

                    type.InvokeMember("Main",
                                        BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public,
                                        null, null,
                                        new object[] { new string[] { } });

                    //reset console redirection
                    Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                    Console.SetIn(new StreamReader(Console.OpenStandardInput()));

                    remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
                    {
                        State = RemoteAppState.Ended
                    });
                }
                catch (Exception ex)
                {
                    remoteExecutionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
                    {
                        State = RemoteAppState.Crashed,
                        Exception = ExceptionDescriptor.FromException(ex)
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
