using Sigged.CodeHost.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    public interface IWorkerService
    {
        event WorkerConnectionHandler WorkerConnected;
        event WorkerConnectionHandler WorkerDisconnected;
        event WorkerMessageReceivedHandler<BuildResultDto> WorkerCompletedBuild;
        event WorkerMessageReceivedHandler<ExecutionStateDto> WorkerExecutionStateChanged;

        bool IsListening { get; }

        void SendWorkerMessage<T>(TcpClient client, MessageType messageType, T message);
        bool StartListening();
        void StopListening();
    }

    public delegate void WorkerConnectionHandler(TcpClient workerClient, string sessionId);
    public delegate void WorkerMessageReceivedHandler<T>(TcpClient workerClient, T message);
}
