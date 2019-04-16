using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.CodeAnalysis.Emit;
using Sigged.CodeHost.Core.Dto;
using Sigged.CsCNetCore.Web.Models;
using Sigged.CsCNetCore.Web.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.CsCNetCore.Web.Sockets
{
    public class CodeHub : Hub
    {
        private RemoteCodeSessionManager _rcsm;

        public CodeHub(RemoteCodeSessionManager rcsm)
        {
            _rcsm = rcsm;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }


        /// <summary>
        /// Receives build requests from user
        /// </summary>
        /// <param name="buildRequest"></param>
        /// <returns></returns>
        public async Task Build(BuildRequestDto buildRequest)
        {
            buildRequest.SessionId = Context.ConnectionId;
            await _rcsm.ProcessUserBuildRequest(buildRequest);
        }
        

        /// <summary>
        /// Receives Console Input from user
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public void ClientInput(string input)
        {
            _rcsm.ForwardConsoleInput(new RemoteInputDto {
                SessionId = Context.ConnectionId,
                Input = input
            });
        }

        /// <summary>
        /// Cancels all actions for this sessio,n
        /// </summary>
        /// <param name="buildRequest"></param>
        /// <returns></returns>
        public async Task StopAll()
        {
            await Task.Run(() =>
            {
                _rcsm.CancelSessionActions(Context.ConnectionId);
            });
        }

        /// <summary>
        /// Should only be used from the server side
        /// </summary>
        /// <returns></returns>
        public async Task DispatchAppStateToClient(string targetConnectionId, ExecutionStateDto state)
        {
            Console.WriteLine($"CodeHub: Sending Appstate to {targetConnectionId} -- {state.State}");
            await Clients.Client(targetConnectionId).SendAsync("ApplicationStateChanged", state);
        }

        /// <summary>
        /// Should only be used from the server side
        /// </summary>
        /// <returns></returns>
        public async Task DispatchBuildResultToClient(string targetConnectionId, BuildResultDto result)
        {
            Console.WriteLine($"CodeHub: Sending BuildResult to {targetConnectionId} -- {result.IsSuccess}");
            await Clients.Client(targetConnectionId).SendAsync("BuildComplete", result);
        }
    }
}
