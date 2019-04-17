using Sigged.CodeHost.Core.Dto;
using System.Net.Sockets;

namespace Sigged.CodeHost.Core.Worker
{
    public interface IWorkerService
    {
        event WorkerConnectionHandler WorkerConnected;
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
