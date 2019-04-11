using System;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    public interface IRemoteExecutionCallback
    {
        Task SendExecutionStateChanged(RemoteCodeSession session, RemoteExecutionState state);

    }
}