using ProtoBuf;
using Sigged.CodeHost.Core.Dto;
using Sigged.CodeHost.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    static class TaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NoWarning(this Task t) { }
    }

    public class WorkerTcpListener : IWorkerService
    {
        protected const int workerIdentificationTimeout = 2000;
        protected bool stopListening = true;
        protected TcpListener listener;
        protected List<TcpClient> connectedClients = new List<TcpClient>();

        public event WorkerConnectionHandler WorkerConnected;
        public event WorkerMessageReceivedHandler<BuildResultDto> WorkerCompletedBuild;
        public event WorkerMessageReceivedHandler<ExecutionStateDto> WorkerExecutionStateChanged;

        protected int listenPort;
        protected IPAddress listenIp;

        public bool IsListening {
            get
            {
                return !stopListening;
            }
        }

        public WorkerTcpListener(IPAddress listenIp, int listenPort)
        {
            this.listenPort = listenPort;
            this.listenIp = listenIp;
        }

        public virtual bool StartListening()
        {
            if (!stopListening)
            {
                Debug.WriteLine("Listener running already");
                return false;
            }
            stopListening = false;

            try
            {
                listener = new TcpListener(listenIp, listenPort);
                listener.Start();

                Thread thread = new Thread(new ThreadStart(ListenLoop));
                thread.IsBackground = true;
                thread.Name = "Worker Listener Thread";
                thread.Start();

                return true;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            return false;
        }

        public virtual void StopListening()
        {
            stopListening = true;
            //networkStream.WriteTimeout = 10;
        }
        
        /// <summary>
        /// Sends a message to worker process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">The TCP client object</param>
        /// <param name="messageType">The message type to send</param>
        /// <param name="message">Actual message payload, which should match message type</param>
        public void SendWorkerMessage<T>(TcpClient client, MessageType messageType, T message)
        {
            var networkStream = client.GetStream();
            networkStream.WriteByte((byte)messageType);
            Serializer.SerializeWithLengthPrefix(networkStream, message, PrefixStyle.Fixed32);

            Console.WriteLine($"SERVER: sent {messageType} to worker");
        }

        protected virtual async void ListenLoop() {
            try
            {
                while (!stopListening)
                {
                    Debug.WriteLine("LISTENER: Waiting for new connection...");
                    var tcpClient = await listener.AcceptTcpClientAsync();
                    Debug.WriteLine($"LISTENER: connection from {tcpClient.Client.RemoteEndPoint}");

                    Task.Run(() => {
                        HandleWorker(tcpClient);
                    })
                    .NoWarning();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                stopListening = true;
                listener?.Stop();
            }
        }

        protected virtual void HandleWorker(TcpClient tcpClient)
        {
            Stream networkStream = null;
            try
            {
                networkStream = tcpClient.GetStream();
                Debug.WriteLine("LISTENER: waiting for worker identification");

                int originalReadTimeout = networkStream.ReadTimeout;
                networkStream.ReadTimeout = workerIdentificationTimeout;

                //wait for client to identify
                IdentificationDto identification;
                try
                {
                    byte msgHeader = (byte)networkStream.ReadByte();
                    MessageType msgType = (MessageType)msgHeader;
                    if (msgType == MessageType.WorkerIdentification)
                    {
                        identification = Serializer.DeserializeWithLengthPrefix<IdentificationDto>(networkStream, PrefixStyle.Fixed32);
                        WorkerConnected?.Invoke(tcpClient, identification.SessionId);

                        Debug.WriteLine($"LISTENER: {tcpClient.Client.RemoteEndPoint} identified as session {identification.SessionId}");
                    }
                    else
                    {
                        identification = null;
                    }
                }
                catch(Exception ex)
                {
                    //most likely the client didn't response properly
                    Debug.WriteLine($"LISTENER: {tcpClient.Client.RemoteEndPoint} did not properly identify with a session: {ex.Message}");
                    identification = null;
                }
                finally
                {
                    networkStream.ReadTimeout = originalReadTimeout;
                }
                

                if(identification == null)
                {
                    //connected worker failed to identify with a sessionid
                    networkStream.Close();
                    networkStream.Dispose();
                    return;
                }
                else
                {
                    bool stopClient = false;

                    while (!stopClient)
                    {
                        //check if client sent data
                        if (tcpClient.Available > 0)
                        {
                            byte msgHeader = (byte)networkStream.ReadByte();
                            MessageType msgType = (MessageType)msgHeader;

                            switch (msgType)
                            {
                                case MessageType.WorkerBuildResult:
                                    var result = Serializer.DeserializeWithLengthPrefix<BuildResultDto>(networkStream, PrefixStyle.Fixed32);
                                    Console.WriteLine("SERVER: received build result");

                                    WorkerCompletedBuild?.Invoke(tcpClient, result);

                                    break;
                                case MessageType.WorkerExecutionState:
                                    var execState = Serializer.DeserializeWithLengthPrefix<ExecutionStateDto>(networkStream, PrefixStyle.Fixed32);
                                    Console.WriteLine($"SERVER: received ExecutionStateDto: {execState?.State}");

                                    if (execState != null)
                                    {
                                        WorkerExecutionStateChanged?.Invoke(tcpClient, execState);


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
                                            case RemoteAppState.WaitForInput:
                                                Console.WriteLine($"SERVER: received remote INPUT request: ");
                                                break;
                                            case RemoteAppState.WaitForInputLine:
                                                Console.WriteLine($"SERVER: received remote INPUTLINE request: ");
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
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"LISTENER: Exception: {ex.Message}");
            }
            finally
            {
                networkStream.Close();
                Console.WriteLine("LISTENER: Ended client connection");
            }
        }

    }
}
