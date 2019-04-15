using ProtoBuf;
using Sigged.CodeHost.Core.Dto;
using Sigged.CodeHost.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sigged.Compling.Core.CodeHost
{
    static class TaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NoWarning(this Task t) { }
    }

    public class BuilderTcpListener
    {
        private bool stopListening = true;
        private TcpListener listener;
        private List<TcpClient> connectedClients = new List<TcpClient>();

        public int Port { get; private set; }
        public string IpAddress { get; private set; }
        public string ThreadName { get; private set; }

        public IEnumerable<TcpClient> ConnectedClients {
            get {
                return connectedClients;
            }
        }

        public BuilderTcpListener(string ipAddress, int port, string threadName)
        {
            Port = port;
            IpAddress = ipAddress;
            ThreadName = threadName;
        }

        public bool Connect()
        {
            if (!stopListening)
            {
                Console.WriteLine("Listener running already");
                return false;
            }
            stopListening = false;

            try
            {
                listener = new TcpListener(IPAddress.Parse(IpAddress), Port);
                listener.Start();

                Thread thread = new Thread(new ThreadStart(ListenLoop));
                thread.IsBackground = true;
                thread.Name = ThreadName;
                thread.Start();

                return true;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public void Disconnect()
        {
            stopListening = true;
            //networkStream.WriteTimeout = 10;
        }

        private async void ListenLoop() {
            try
            {
                while (!stopListening)
                {
                    Console.WriteLine("Waiting for new connection...");
                    var tcpClient = await listener.AcceptTcpClientAsync();
                    Console.WriteLine($"New builder connected: {tcpClient.Client.RemoteEndPoint}");

                    Task.Run(() => {
                        HandleClientAsync(tcpClient);
                    })
                    .NoWarning();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                stopListening = true;
                listener?.Stop();
            }
        }

        private void HandleClientAsync(TcpClient tcpClient)
        {
            try
            {
                var networkStream = tcpClient.GetStream();
                Console.WriteLine("SERVER: sending build request");
                //send build request
                networkStream.WriteByte((byte)MessageType.ServerBuildRequest);
                Serializer.SerializeWithLengthPrefix(networkStream, new BuildRequestDto
                {
                    SessionId = "session-id-dummy",
                    RunOnSuccess = true,
                    SourceCode = @"
/* C# demo code */
using System;

namespace Test {

    public class Program {
    
        public static void Main(string[] args) 
        {
            Console.Write(""What is your\nname ? "");
            //char input = (char)Console.Read();
            string input = Console.ReadLine();
            Console.WriteLine($""Hello { input }"");
            Console.WriteLine($""Nice to meet you"");
            //int i = 0, j = 1;
            //i = j / i;
        }
    }
}
                    "
                }, PrefixStyle.Fixed32);

               

                bool stopClient = false;

                while (!stopClient)
                {
                    //check if client sent data
                    if(tcpClient.Available > 0)
                    {
                        byte msgHeader = (byte)networkStream.ReadByte();
                        MessageType msgType = (MessageType)msgHeader;

                        switch (msgType)
                        {
                            case MessageType.WorkerBuildResult:
                                //wait for build results
                                Console.WriteLine("SERVER: sent build request, waiting for results...");
                                var result = Serializer.DeserializeWithLengthPrefix<BuildResultDto>(networkStream, PrefixStyle.Fixed32);

                                Console.WriteLine("SERVER: received build result");
                                break;
                            case MessageType.WorkerExecutionState:
                                var execState = Serializer.DeserializeWithLengthPrefix<ExecutionStateDto>(networkStream, PrefixStyle.Fixed32);
                                Console.WriteLine($"SERVER: received ExecutionStateDto: {execState?.State}");

                                if(execState != null)
                                {
                                    switch (execState.State)
                                    {
                                        case RemoteAppState.Running:
                                            break;
                                        case RemoteAppState.Ended:
                                            break;
                                        case RemoteAppState.NotRunning:
                                            break;
                                        case RemoteAppState.Crashed:
                                            Console.WriteLine($"SERVER: received remote crash info: {execState.Exception.Name}");
                                            break;
                                        case RemoteAppState.WriteOutput:
                                            string printableOutput = execState.Output?
                                                .Replace("\r", "\\r")?
                                                .Replace("\n", "\\n"); //simply for visualizing special chars
                                            Console.WriteLine($"SERVER: received remote output info: {printableOutput}");
                                            break;
                                        case RemoteAppState.WaitForInputLine:
                                            string input = null;
                                            Console.WriteLine($"SERVER: received remote INPUTLiNE request: ");
                                            input = Console.ReadLine();
                                            networkStream.WriteByte((byte)MessageType.ServerRemoteInput);
                                            Serializer.SerializeWithLengthPrefix(networkStream, new RemoteInputDto
                                            {
                                                SessionId = "blah",
                                                Input = input
                                            }, PrefixStyle.Fixed32);

                                            Console.WriteLine($"SERVER: sent input to client: {input}");

                                            break;
                                        default:
                                            Console.WriteLine($"SERVER: received unsupported ExecutionState: {execState.State}");
                                            break;
                                    }
                                }
                                else
                                {
                                    stopClient = true;
                                    Console.WriteLine("SERVER: client send execstate NULL, STOPPING comms");
                                }

                                break;
                            default:
                                Console.WriteLine($"SERVER: received client message header: {msgHeader}");
                                break;
                        }
                    }
                }

                networkStream.Close();
                Console.WriteLine("Ended client connection");
            }
            finally
            {
                Console.WriteLine("Finalized listener");
                
            }
        }
    }
}
