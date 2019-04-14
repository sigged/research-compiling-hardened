using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using ProtoBuf;
using Sigged.CodeHost.Core.Dto;
using Sigged.CodeHost.Core.Serialization;
using Sigged.Compiling.Core;

namespace Sigged.CodeHost.Worker
{
    class Program
    {

        static WorkerClient client;
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

        //static void Main(string[] args)
        //{
        //    //gather arguments
        //    string huburl = "https://localhost:5001/workerHub"; //args[0];
        //    string sessionid = "bogus-session-id"; //args[1];

        //    compiler = new Compiler(@"D:\BaTi\Thesis\Projects\Sigged.Compiling\Compile.and.Load\src\Sigged.Repl.NetCore.Web\_libs\netstandard2.0");

        //    client = new WorkerClient(huburl, sessionid, compiler);
        //    try
        //    {
        //        Task.Delay(5000).Wait();
        //        client.Connect().Wait();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");
        //    }

        //}

        static void Main(string[] args)
        {
            //gather arguments
            //string sessionid = "bogus-session-id";//args[0];
            //string pipeName = "codehost21.pipe"; //args[1];
            string host = "localhost"; //args[0];
            int port = 2000; //args[1];
            string sessionid = "bogus-session-id"; //args[2];

            Compiler compiler = new Compiler(@"D:\BaTi\Thesis\Projects\Sigged.Compiling\Compile.and.Load\src\Sigged.Repl.NetCore.Web\_libs\netstandard2.0");


            

            TcpClient client = null;
            NetworkStream networkStream = null;
            try
            {
                using (client = new TcpClient())
                {
                    client.Connect(host, port);

                    using (networkStream = client.GetStream())
                    {
                        Debug.WriteLine("CLIENT: waiting for server...");

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
                                        Debug.WriteLine("CLIENT: received BuildRequestDto");

                                        EmitResult results = null;
                                        byte[] assemblyBytes = null;
                                        using (var assemblyStream = new MemoryStream())
                                        {
                                            results = compiler.Compile(buildrequest.SourceCode, buildrequest.SessionId, assemblyStream,
                                                outputKind: OutputKind.ConsoleApplication).Result;

                                            assemblyBytes = assemblyStream.ToArray();
                                        }
                                        Debug.WriteLine("CLIENT: built source code");

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

                                        networkStream.WriteByte((byte)MessageType.ClientBuildResult);
                                        Serializer.SerializeWithLengthPrefix(networkStream, result, PrefixStyle.Fixed32);
                                        Debug.WriteLine("CLIENT: sent buid result");

                                        if(buildrequest.RunOnSuccess && result.IsSuccess)
                                        {
                                            RunApplication(sessionid, client, assemblyBytes);
                                        }

                                        break;
                                    default:
                                        Debug.WriteLine($"Unknown server message header: {msgHeader}");
                                        break;
                                }
                            }
                            
                        }
                    }

                    //networkStream = client.GetStream();

                    //Console.WriteLine("CLIENT: awaiting BuildRequestDto");
                    //var buildrequest = Serializer.DeserializeWithLengthPrefix<BuildRequestDto>(networkStream, PrefixStyle.Fixed32);
                    //Console.WriteLine("CLIENT: received BuildRequestDto");

                    //EmitResult results = null;
                    //byte[] assemblyBytes = null;
                    //using (var assemblyStream = new MemoryStream())
                    //{
                    //    results = compiler.Compile(buildrequest.SourceCode, buildrequest.SessionId, assemblyStream,
                    //        outputKind: OutputKind.ConsoleApplication).Result;

                    //    assemblyBytes = assemblyStream.ToArray();
                    //}

                    //Console.WriteLine("CLIENT: built source code");

                    //BuildResultDto result = new BuildResultDto();
                    //result.SessionId = buildrequest.SessionId;
                    //result.BuildErrors = results.Diagnostics.Select(d =>
                    //    new BuildErrorDto
                    //    {
                    //        Id = d.Id,
                    //        Severity = d.Severity.ToString()?.ToLower(),
                    //        Description = d.GetMessage(),
                    //        StartPosition = LinePositionDto.FromLinePosition(d.Location.GetLineSpan().StartLinePosition),
                    //        EndPosition = LinePositionDto.FromLinePosition(d.Location.GetLineSpan().EndLinePosition),
                    //    }).ToList();

                    //result.IsSuccess = results.Success;

                    //Serializer.SerializeWithLengthPrefix(networkStream, result, PrefixStyle.Fixed32);
                    //Console.WriteLine("CLIENT: sent build result");



                    //if (result.IsSuccess)
                    //{
                    //    IsRunning = true;
                    //    RunApplication(sessionid, networkStream, assemblyBytes);
                    //    IsRunning = false;
                    //}
                    //else
                    //{
                    //    //notify end of work
                    //    var execState = new ExecutionStateDto
                    //    {
                    //        SessionId = sessionid,
                    //        State = RemoteAppState.NotRunning
                    //    };
                    //    Serializer.SerializeWithLengthPrefix(networkStream, execState, PrefixStyle.Fixed32);
                    //}

                    //while (IsRunning)
                    //{
                    //    Task.Delay(100).Wait();
                    //    var remoteInput = Serializer.DeserializeWithLengthPrefix<RemoteInputDto>(networkStream, PrefixStyle.Fixed32);
                    //}

                    //Console.WriteLine(Environment.NewLine + "client: shutting down");

                }
            }
            finally
            {
                networkStream?.Close();
                networkStream?.Dispose();
                client?.Close();
                client?.Dispose();
            }

            Console.ReadLine();
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
                networkStream.WriteByte((byte)MessageType.ClientExectionState);
                Serializer.SerializeWithLengthPrefix(networkStream, execState, PrefixStyle.Fixed32);
                Debug.WriteLine($"CLIENT: sent execution state {execState.State}");


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
                networkStream.WriteByte((byte)MessageType.ClientExectionState);
                Serializer.SerializeWithLengthPrefix(networkStream, execState, PrefixStyle.Fixed32);
                Debug.WriteLine($"CLIENT: sent execution state {execState.State}");

            }
            catch (SocketException socketEx)
            {
                Debug.WriteLine($"CLIENT Error: {socketEx.Message}");
            }
            catch (Exception ex)
            {
                execState = new ExecutionStateDto
                {
                    SessionId = sessionid,
                    State = RemoteAppState.Crashed,
                    Exception = ExceptionDto.FromException(ex)
                };
                networkStream.WriteByte((byte)MessageType.ClientExectionState);
                Serializer.SerializeWithLengthPrefix(networkStream, execState, PrefixStyle.Fixed32);
            }
            finally
            {

            }
        }

        private static void HandleServerComm(TcpClient tcpClient)
        {
            
        }
    }
}
