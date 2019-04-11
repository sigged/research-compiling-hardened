using Microsoft.CodeAnalysis.Emit;
using System;
using System.Threading;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class RemoteCodeSession
    {
        public string SessionId { get; set; }
        public EmitResult LastResult { get; set; }
        public byte[] LastAssembly { get; set; }
        public Thread ExecutionThread { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public bool IsBuilding { get; set; }
        public bool IsRunning { get; set; }
    }
}
