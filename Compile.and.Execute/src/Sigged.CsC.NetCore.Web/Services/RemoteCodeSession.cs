using Sigged.CodeHost.Core.Dto;
using Sigged.CsC.NetCore.Web.Models;
using System;
using System.Net.Sockets;

namespace Sigged.CsC.NetCore.Web.Services
{
    public class RemoteCodeSession
    {
        public RemoteCodeSession()
        {
            LastAppState = RemoteAppState.NotRunning;
            Heartbeat();
        }

        public string SessionId { get; set; }
        public BuildRequestDto LastBuildRequest { get; set; }
        public RemoteAppState LastAppState { get; set; }
        public IWorkerProcess WorkerProcess { get; set; }
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
