using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Sigged.CodeHost.Core.Dto;
using Sigged.CsCNetCore.Web.Sockets;

namespace Sigged.CsCNetCore.Web.Services
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

            ////try to detect local kestrel ports, this is useful behind a docker containers port redirections (e.g. -p 8080:80)
            //string localListeningAddress = ServerInfoService.GetFirstNonSecureLocalAddress();
            //if(localListeningAddress == null) 
            //{
            //    //kestrel ports not configured, it could mean web run behind reverse proxy such as IIS
            //    //see ref: https://github.com/aspnet/Hosting/issues/811
                
            //    appBaseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}";
            //}
            //else
            //{
            //    appBaseUrl = $"{localListeningAddress}";
            //}
            hubConnection = new HubConnectionBuilder().WithUrl(appBaseUrl + "/codeHub").Build();
            Console.WriteLine($"SignalRClientService is using {appBaseUrl}/codeHub");

            hubConnection.Closed += HubConnection_Closed;
        }

        private async Task HubConnection_Closed(Exception arg)
        {
            Console.Write("SignalRClientService lost hub connection, reconnecting...");
            try
            {
                await hubConnection?.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Alert! unable to restart SignalRClientService hubconnection: {ex.Message}");
            }
        }

        public async Task Connect()
        {
            await hubConnection.StartAsync();
            Console.WriteLine($"SignalRClientService: connected to {appBaseUrl}/codeHub");
        }

        public async Task SendBuildResult(string sessionId, BuildResultDto result)
        {
            Console.WriteLine($"SignalRClientService.SendBuildResult: Dispatching BuildResultDto to client..");
            try
            {
                await hubConnection.InvokeAsync(nameof(CodeHub.DispatchBuildResultToClient), sessionId, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalRClientService: {ex.Message}");
            }
        }

        public async Task SendExecutionState(string sessionId, ExecutionStateDto state)
        {
            Console.WriteLine($"SignalRClientService: Dispatching ExecutionStateDto to client..");
            try
            {
                await hubConnection.InvokeAsync(nameof(CodeHub.DispatchAppStateToClient), sessionId, state);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalRClientService.SendExecutionState: {ex.Message}");
            }
        }

        ~SignalRClientService()
        {
            Console.WriteLine($"SignalRClientService: Disposing.");
            hubConnection?.DisposeAsync()?.Wait();
        }
    }
}
