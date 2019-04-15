using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using ProtoBuf;
using Sigged.CodeHost.Core.Dto;
using Sigged.Compiling.Core;

namespace Sigged.CodeHost.Worker
{
    class Program
    {

        static Compiler compiler;
        static Thread execThread;

        private static bool stopClient = false;


        private static int _isRunning = 0; //back value of thread safe IsRunning prop

        public static bool IsRunning
        {
            get { return (Interlocked.CompareExchange(ref _isRunning, 1, 1) == 1); }
            set
            {
                if (value) Interlocked.CompareExchange(ref _isRunning, 1, 0);
                else Interlocked.CompareExchange(ref _isRunning, 0, 1);
            }
        }

        static void Main(string[] args)
        {
            //gather arguments
            string host = args[0]; // "localhost"; //args[0];
            int port = int.Parse(args[1]); // 2000; //args[1];
            string sessionid = args[2]; // "bogus-session-id"; //args[2];

            //Console.Write($"Press enter to connect to {host}:{port} as {sessionid}");
            //Console.ReadLine();

            string netstandardLibs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "buildlibs", "netstandard2.0");
            Compiler compiler = new Compiler(netstandardLibs);

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
                        Serializer.SerializeWithLengthPrefix(networkStream, new IdentificationDto {
                            SessionId = sessionid
                        }, PrefixStyle.Fixed32);

                        Logger.LogLine("CLIENT: waiting for server...");

                        stopClient = false;
                        while (!stopClient)
                        {
                            //check if server sent data
                            if(client.Available > 0)
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
                                        using (var assemblyStream = new MemoryStream())
                                        {
                                            results = compiler.Compile(buildrequest.SourceCode, buildrequest.SessionId, assemblyStream,
                                                outputKind: OutputKind.ConsoleApplication).Result;

                                            assemblyBytes = assemblyStream.ToArray();
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

                                        if(buildrequest.RunOnSuccess && result.IsSuccess)
                                        {
                                            RunApplication(sessionid, client, assemblyBytes);
                                            
                                        }

                                        //done processing the server request
                                        //stopClient = true;

                                        break;
                                    default:
                                        Logger.LogLine($"Unknown server message header: {msgHeader}");
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

                //Console.ReadLine();
            }

            
        }

        static void RunApplication(string sessionid, TcpClient client, byte[] assemblyBytes)
        {
            var networkStream = client.GetStream();
            var outputRedirector = new ConsoleOutputService(sessionid, client);
            var inputRedirector = new ConsoleInputService(sessionid, client);

            ExecutionStateDto execState;
            var assemly = Assembly.Load(assemblyBytes);
            var type = assemly.GetType("Test.Program");
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

                type.InvokeMember("Main",
                                    BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public,
                                    null, null,
                                    new object[] { new string[] { } });

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
