using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class RemoteExecutionState
    {
        public RemoteAppState State { get; set; }
        

    }

    public enum RemoteAppState
    {
        NotRunning = 0,
        Running = 1,
        WaitForInput = 2,
        Crashed = 3,
        Ended = 4
    }
}
