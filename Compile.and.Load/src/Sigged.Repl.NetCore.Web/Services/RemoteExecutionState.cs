using Sigged.Repl.NetCore.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class RemoteExecutionState
    {
        public RemoteAppState State { get; set; }
        public ExceptionDescriptor Exception { get; set; }
        public string Output { get; set; }
        
    }

    public enum RemoteAppState
    {
        NotRunning = 0,
        Running = 1,
        WriteOutput = 10,
        WaitForInput = 11,
        WaitForInputLine = 12,
        Crashed = 20,
        Ended = 100
    }
}
