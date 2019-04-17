using Sigged.CodeHost.Core.Worker;
using Sigged.CodeHost.Worker.Tests.Mock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Sigged.CodeHost.Worker.Tests.Models
{
    internal class WorkerClientArrangement
    {
        private static int lastListenPort = 1999; //ensures all test get a unique port on concurrent execution

        public WorkerClientArrangement()
        {
            string srcParentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                .Parent.Parent.Parent.Parent.Parent.FullName;

            string netstandardLibPath = Path.Combine(srcParentDir, "libs", "netstandard2.0");

            ServiceHostName = "localhost";
            ServicePort = ++lastListenPort;
            WorkerService = new MockWorkerTcpListener(IPAddress.Any, ServicePort);
            Worker = new Worker(netstandardLibPath);
            SessionId = "dummy-session-id";
        }

        public string ServiceHostName { get; private set; }
        public int ServicePort { get; private set; }
        public IWorkerService WorkerService { get; private set; }
        public IWorker Worker { get; private set; }
        public string SessionId { get; private set; }
    }
}
