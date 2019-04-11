using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    /// <summary>
    /// Handles compilation and run requests for web sessions
    /// </summary>
    public interface IRemoteCodeSessionManager
    {
        event RemoteApplicationStateChangedHandler AppStateChanged;

        IEnumerable<RemoteCodeSession> Sessions { get; }

        void CleanupIdleSessions();

        RemoteCodeSession CreateSession(string uniqueSessionId);
        
        RemoteCodeSession GetSession(string sessionid);

        Task<EmitResult> Compile(string sessionid, string code);

        void RunLastCompilation(string sessionid);
    }
}
