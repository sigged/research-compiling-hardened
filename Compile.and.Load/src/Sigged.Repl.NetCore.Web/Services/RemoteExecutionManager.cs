using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Sigged.Repl.NetCore.Web.Sockets;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class SignalRRemoteExecutionCallback : IRemoteExecutionCallback
    {
        protected IHttpContextAccessor httpContextAccessor;

        public SignalRRemoteExecutionCallback(IHttpContextAccessor httpContextAcces)
        {
            httpContextAccessor = httpContextAcces;
        }

        public async Task SendExecutionStateChanged(RemoteCodeSession session, RemoteExecutionState state)
        {
            var currentContext = httpContextAccessor.HttpContext;
            string appBaseUrl = $"{currentContext.Request.Scheme}://{currentContext.Request.Host}{currentContext.Request.PathBase}";

            //use a server-side client to connect to the hub endpoint an send the message
            var hubClientConnection = new HubConnectionBuilder().WithUrl("https://localhost:44341/codeHub").Build();
            await hubClientConnection.StartAsync();

            //call hub endpoint and tell it to dispatch app state to remote client
            await hubClientConnection.InvokeAsync(nameof(CodeHub.DispatchAppStateToRemoteClient), session.SessionId, state);
        }
    }
}
