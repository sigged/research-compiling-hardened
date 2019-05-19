using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using ProtoBuf;
using Sigged.CodeHost.Core.Dto;
using Sigged.CodeHost.Core.Worker;
using Sigged.CodeHost.Core.Logging;
using Sigged.Compiling.Core;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Sigged.CodeHost.Worker
{
    public class Worker : IWorker
    {
        private bool stopClient = false;
        private string netStandardLibPath = null;

        public Worker()
        {
            netStandardLibPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "buildlibs", "netstandard2.0");
        }

        public Worker(string netStandardLibPath)
        {
            this.netStandardLibPath = netStandardLibPath;
        }

        public async Task Start(string host, int port, string sessionid)
        {
            await Task.Run(() => StartInternal(host, port, sessionid));
        }

        public void Stop()
        {
            stopClient = true;
        }

        private void StartInternal(string host, int port, string sessionid)
        {
            Compiler compiler = new Compiler(netStandardLibPath);

            TcpClient client = null;
            NetworkStream networkStream = null;
            try
            {
                using (client = new TcpClient())
                {
                    client.Connect(host, port);

                    using (networkStream = client.GetStream())
                    {
                        Logger.LogLine("CLIENT: identifying with server...");
                        networkStream.WriteByte((byte)MessageType.WorkerIdentification);
                        Serializer.SerializeWithLengthPrefix(networkStream, new IdentificationDto
                        {
                            SessionId = sessionid
                        }, PrefixStyle.Fixed32);

                        Logger.LogLine("CLIENT: waiting for server...");

                        stopClient = false;
                        while (!stopClient)
                        {
                            if (!client.Connected)
                            {
                                stopClient = true;
                                break;
                            }

                            //check if server sent data
                            if (client.Available > 0)
                            {
                                byte msgHeader = (byte)networkStream.ReadByte();
                                MessageType msgType = (MessageType)msgHeader;

                                switch (msgType)
                                {
                                    case MessageType.ServerBuildRequest:
                                        //build shit
                                        var buildrequest = Serializer.DeserializeWithLengthPrefix<BuildRequestDto>(networkStream, PrefixStyle.Fixed32);
                                        Logger.LogLine("CLIENT: received BuildRequestDto");

                                        EmitResult results = null;
                                        byte[] assemblyBytes = null;

                                        try
                                        {
                                            
                                            using (var assemblyStream = new MemoryStream())
                                            {
                                                results = compiler.Compile(buildrequest.SourceCode, buildrequest.SessionId, assemblyStream,
                                                    outputKind: OutputKind.ConsoleApplication).Result;

                                                assemblyBytes = assemblyStream.ToArray();
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            Logger.LogLine($"CLIENT: Error! {ex.Message}");
                                            throw;
                                        }
                                        
                                        Logger.LogLine("CLIENT: built source code");

                                        BuildResultDto result = new BuildResultDto();
                                        result.SessionId = buildrequest.SessionId;
                                        result.BuildErrors = results.Diagnostics.Select(d =>
                                            new BuildErrorDto
                                            {
                                                Id = d.Id,
                                                Severity = d.Severity.ToString()?.ToLower(),
                                                Description = d.GetMessage(),
                                                StartPosition = LinePositionDto.FromLinePosition(d.Location.GetLineSpan().StartLinePosition),
                                                EndPosition = LinePositionDto.FromLinePosition(d.Location.GetLineSpan().EndLinePosition),
                                            }).ToList();

                                        result.IsSuccess = results.Success;

                                        networkStream.WriteByte((byte)MessageType.WorkerBuildResult);
                                        Serializer.SerializeWithLengthPrefix(networkStream, result, PrefixStyle.Fixed32);
                                        Logger.LogLine("CLIENT: sent build result");

                                        if (buildrequest.RunOnSuccess && result.IsSuccess)
                                        {
                                            RunApplication(sessionid, client, assemblyBytes);
                                        }

                                        break;
                                    default:
                                        Logger.LogLine($"CLIENT: ERROR! Unknown server message header: {msgHeader}");
                                        Logger.LogLine($"CLIENT: Shutting down due to unexpected server message.");
                                        stopClient = true;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                networkStream?.Close();
                networkStream?.Dispose();
                client?.Close();
                client?.Dispose();
            }
        }

        public void RunApplication(string sessionid, TcpClient client, byte[] assemblyBytes)
        {
            var networkStream = client.GetStream();
            var outputRedirector = new ConsoleOutputService(sessionid, client);
            var inputRedirector = new ConsoleInputService(sessionid, client);

            ExecutionStateDto execState;
            var assembly = Assembly.Load(assemblyBytes);
            try
            {
                execState = new ExecutionStateDto
                {
                    SessionId = sessionid,
                    State = RemoteAppState.Running
                };
                networkStream.WriteByte((byte)MessageType.WorkerExecutionState);
                Serializer.SerializeWithLengthPrefix(networkStream, execState, PrefixStyle.Fixed32);
                Logger.LogLine($"CLIENT: sent execution state {execState.State}");

                //redirect console
                Console.SetOut(outputRedirector);
                Console.SetIn(inputRedirector);

                //invoke main method
                var mainParms = assembly.EntryPoint.GetParameters();
                if(mainParms.Count() == 0)
                {
                    assembly.EntryPoint.Invoke(null, null);
                }
                else
                {
                    if(mainParms[0].ParameterType == typeof(string[]))
                        assembly.EntryPoint.Invoke(null, new string[] { null });
                    else
                        assembly.EntryPoint.Invoke(null, null);
                }

                //reset console redirection
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                Console.SetIn(new StreamReader(Console.OpenStandardInput()));

                execState = new ExecutionStateDto
                {
                    SessionId = sessionid,
                    State = RemoteAppState.Ended
                };
                networkStream.WriteByte((byte)MessageType.WorkerExecutionState);
                Serializer.SerializeWithLengthPrefix(networkStream, execState, PrefixStyle.Fixed32);
                Logger.LogLine($"CLIENT: sent execution state {execState.State}");

            }
            catch (SocketException socketEx)
            {
                Logger.LogLine($"CLIENT Error: {socketEx.Message}");
            }
            catch (Exception ex)
            {
                execState = new ExecutionStateDto
                {
                    SessionId = sessionid,
                    State = RemoteAppState.Crashed,
                    Exception = ExceptionDto.FromException(ex)
                };
                networkStream.WriteByte((byte)MessageType.WorkerExecutionState);
                Serializer.SerializeWithLengthPrefix(networkStream, execState, PrefixStyle.Fixed32);
            }
            finally
            {

            }
        }
    }
}
