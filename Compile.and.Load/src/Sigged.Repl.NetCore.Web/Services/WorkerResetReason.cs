using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    public enum WorkerResetReason
    {
        WorkerStopped = 1,
        UserCancelled = 2,
        Expired = 10
    }
}
