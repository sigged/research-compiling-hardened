using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Sigged.CodeHost.Core.Dto;
using Sigged.Repl.NetCore.Web.Sockets;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class SignalRClientService : IClientService
    {
        protected HttpContext httpContext;
        protected string appBaseUrl;
        protected HubConnection hubConnection;

        public SignalRClientService(IHttpContextAccessor httpAccessor)
        {
            httpContext = httpAccessor.HttpContext;
            appBaseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}";
            hubConnection = new HubConnectionBuilder().WithUrl(appBaseUrl + "/codeHub" /*"https://localhost:44341/codeHub"*/).Build();
        }

        public async Task Connect()
        {
            await hubConnection.StartAsync();
        }

        public async Task SendBuildResult(string sessionId, BuildResultDto result)
        {
            await hubConnection.InvokeAsync(nameof(CodeHub.DispatchBuildResultToClient), sessionId, result);
        }

        public async Task SendExecutionState(string sessionId, ExecutionStateDto state)
        {
            await hubConnection.InvokeAsync(nameof(CodeHub.DispatchAppStateToClient), sessionId, state);
        }

        ~SignalRClientService()
        {
            hubConnection?.DisposeAsync()?.Wait();
        }
    }
}
