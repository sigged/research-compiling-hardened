using Microsoft.CodeAnalysis.Emit;
using Sigged.CodeHost.Core.Dto;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class RemoteCodeSession
    {
        //public RemoteCodeSession(IRemoteExecutionCallback executionCallback)
        //{
        //    this.consoleOutputRedirector = new ConsoleOutputService(this, executionCallback);
        //    this.consoleInputRedirector = new ConsoleInputService(this, executionCallback);
        //}
        public string SignalRConnectionId { get; set; }
        public string SessionId { get; set; }
        public BuildRequestDto LastBuildRequest { get; set; }
        public EmitResult LastResult { get; set; }
        public byte[] LastAssembly { get; set; }
        //public Thread ExecutionThread { get; set; }
        public Process WorkerProcess { get; set; }
        public TcpClient WorkerClient { get; set; }
        //public ConsoleOutputService consoleOutputRedirector { get; private set; }
        //public ConsoleInputService consoleInputRedirector { get; private set; }
        public DateTimeOffset LastActivity { get; set; }
        public bool IsBuilding { get; set; }
        public bool IsRunning { get; set; }
    }
}
