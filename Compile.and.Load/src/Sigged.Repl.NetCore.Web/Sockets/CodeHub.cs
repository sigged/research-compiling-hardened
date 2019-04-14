using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.CodeAnalysis.Emit;
using Sigged.Repl.NetCore.Web.Models;
using Sigged.Repl.NetCore.Web.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Sockets
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


        private async Task<BuildResultDto> BuildCore(BuildRequestDto buildRequest)
        {
            BuildResultDto result = new BuildResultDto();
            result.SessionId = Context.ConnectionId;
            EmitResult results = await _rcsm.Compile(Context.ConnectionId, buildRequest.SourceCode);
            result.BuildErrors = results.Diagnostics.Select(d =>
                new BuildErrorDto
                {
                    Id = d.Id,
                    Severity = d.Severity.ToString()?.ToLower(),
                    Description = d.GetMessage(),
                    StartPosition = d.Location.GetLineSpan().StartLinePosition,
                    EndPosition = d.Location.GetLineSpan().EndLinePosition
                }).ToList();

            result.IsSuccess = results.Success;
            return result;
        }

        private void RunCore(BuildRequestDto buildRequest)
        {
            _rcsm.RunLastCompilation(Context.ConnectionId);
        }

        public async Task Build(BuildRequestDto buildRequest)
        {
            var session = _rcsm.GetSession(Context.ConnectionId);
            if (session == null)
            {
                session = _rcsm.CreateSession(Context.ConnectionId);
                //buildRequest.CodingSessionId = session.SessionId;
            }
            BuildResultDto result = await BuildCore(buildRequest);
            await Clients.Caller.SendAsync("BuildComplete", result);
        }
        
        public async Task BuildAndRunCode(BuildRequestDto buildRequest)
        {
            var session = _rcsm.GetSession(Context.ConnectionId);
            if (session == null)
            {
                session = _rcsm.CreateSession(Context.ConnectionId);
                //buildRequest.CodingSessionId = session.SessionId;
            }

            //build code
            BuildResultDto result = await BuildCore(buildRequest);
            await Clients.Caller.SendAsync("BuildComplete", result);

            //run code when good build
            if (session.LastResult.Success)
            {
                RunCore(buildRequest);
            }
        }

        public async Task ClientInput(string input)
        {
            var session = _rcsm.GetSession(Context.ConnectionId);
            if(session != null)
            {
                session.consoleInputRedirector.ReceiveInput(input);
            }
        }

        /// <summary>
        /// Should only be used from the server side
        /// </summary>
        /// <returns></returns>
        public async Task DispatchAppStateToRemoteClient(string targetConnectionId, RemoteExecutionState state)
        {
            state.SessionId = targetConnectionId;
            Debug.WriteLine($"Sending Appstate to {targetConnectionId} -- {state.State}");
            await Clients.Client(targetConnectionId).SendAsync("ApplicationStateChanged", targetConnectionId, state);
        }

    }
}
