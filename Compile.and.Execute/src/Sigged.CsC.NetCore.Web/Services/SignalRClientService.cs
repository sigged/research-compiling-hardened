using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Sigged.CodeHost.Core.Dto;
using Sigged.CodeHost.Core.Logging;
using Sigged.CsC.NetCore.Web.Sockets;
using System;
using System.Threading.Tasks;

namespace Sigged.CsC.NetCore.Web.Services
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

            hubConnection = new HubConnectionBuilder().WithUrl(appBaseUrl + "/codeHub").Build();
            Logger.LogLine($"SignalRClientService is using {appBaseUrl}/codeHub");

            hubConnection.Closed += HubConnection_Closed;
        }

        private async Task HubConnection_Closed(Exception arg)
        {
            Logger.LogLine("SignalRClientService lost hub connection, reconnecting...");
            try
            {
                await hubConnection?.StartAsync();
            }
            catch (Exception ex)
            {
                Logger.LogLine($"Alert! unable to restart SignalRClientService hubconnection: {ex.Message}");
            }
        }

        public async Task Connect()
        {
            await hubConnection.StartAsync();
            Logger.LogLine($"SignalRClientService: connected to {appBaseUrl}/codeHub");
        }

        public async Task SendBuildResult(string sessionId, BuildResultDto result)
        {
            Logger.LogLine($"SignalRClientService.SendBuildResult: Dispatching BuildResultDto to client..");
            try
            {
                await hubConnection.InvokeAsync(nameof(CodeHub.DispatchBuildResultToClient), sessionId, result);
            }
            catch (Exception ex)
            {
                Logger.LogLine($"SignalRClientService: {ex.Message}");
            }
        }

        public async Task SendExecutionState(string sessionId, ExecutionStateDto state)
        {
            Logger.LogLine($"SignalRClientService: Dispatching ExecutionStateDto to client..");
            try
            {
                await hubConnection.InvokeAsync(nameof(CodeHub.DispatchAppStateToClient), sessionId, state);
            }
            catch (Exception ex)
            {
                Logger.LogLine($"SignalRClientService.SendExecutionState: {ex.Message}");
            }
        }

        ~SignalRClientService()
        {
            Logger.LogLine($"SignalRClientService: Disposing.");
            hubConnection?.DisposeAsync()?.Wait();
        }
    }
}
