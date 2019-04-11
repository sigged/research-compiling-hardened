using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    public delegate void RemoteApplicationStateChangedHandler(RemoteCodeSession session, RemoteExecutionState args);
    
}
