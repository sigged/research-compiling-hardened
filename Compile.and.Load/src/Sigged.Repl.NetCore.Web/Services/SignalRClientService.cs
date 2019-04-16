using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
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
