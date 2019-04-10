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
        IEnumerable<RemoteCodeSession> Sessions { get; }

        void CleanupIdleSessions();

        RemoteCodeSession CreateSession();
        
        RemoteCodeSession GetSession(string sessionid);

        Task<EmitResult> Compile(string sessionid, string code);

        Task<bool> RunLastCompilation(string sessionid);
    }
}
