using Sigged.CodeHost.Core.Dto;
using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class RemoteCodeSession
    {
        public RemoteCodeSession()
        {
            Heartbeat();
        }

        public string SessionId { get; set; }
        public BuildRequestDto LastBuildRequest { get; set; }
        public Process WorkerProcess { get; set; }
        public TcpClient WorkerClient { get; set; }
        public DateTimeOffset LastActivity { get; private set; }

        /// <summary>
        /// Updates LastActivity time
        /// </summary>
        public void Heartbeat()
        {
            LastActivity = DateTimeOffset.Now;
        }
    }
}
