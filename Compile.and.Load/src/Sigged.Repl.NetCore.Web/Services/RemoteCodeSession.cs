using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class RemoteCodeSession
    {
        public string SessionId { get; set; }
        public EmitResult LastResult { get; set; }
        public byte[] LastAssembly { get; set; }
        public DateTimeOffset LastActivity { get; set; }
    }
}
