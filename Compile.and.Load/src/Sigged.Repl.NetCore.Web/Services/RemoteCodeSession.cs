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
        public string SessionId { get; set; }
        public BuildRequestDto LastBuildRequest { get; set; }
        public Process WorkerProcess { get; set; }
        public TcpClient WorkerClient { get; set; }
        public DateTimeOffset LastActivity { get; set; }
    }
}
