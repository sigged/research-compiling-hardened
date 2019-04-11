using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Emit;
using Sigged.Repl.NetCore.Web.Models;
using Sigged.Repl.NetCore.Web.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Sockets
{
    public class CodeHub : Hub
    {
        private RemoteCodeSessionManager _rcsm;
        //private IHubContext<CodeHub> _hubContext;

        public CodeHub(/*IHubContext<CodeHub> hubContext,*/ RemoteCodeSessionManager rcsm)
        {
            //_hubContext = hubContext;
            _rcsm = rcsm;
            _rcsm.AppStateChanged += RemoteCodeSessionMgr_AppStateChanged;
        }


        private async Task<BuildResultDto> BuildCore(BuildRequestDto buildRequest)
        {
            BuildResultDto result = new BuildResultDto();
            EmitResult results = await _rcsm.Compile(buildRequest.CodingSessionId, buildRequest.SourceCode);
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
            _rcsm.RunLastCompilation(buildRequest.CodingSessionId);
        }

        public async Task Build(BuildRequestDto buildRequest)
        {
            var session = _rcsm.GetSession(buildRequest.CodingSessionId);
            if (session == null)
            {
                session = _rcsm.CreateSession();
                buildRequest.CodingSessionId = session.SessionId;
            }
            BuildResultDto result = await BuildCore(buildRequest);
            await Clients.Caller.SendAsync("BuildComplete", result);
        }

        public async Task BuildAndRunCode(BuildRequestDto buildRequest)
        {
            var session = _rcsm.GetSession(buildRequest.CodingSessionId);
            if (session == null)
            {
                session = _rcsm.CreateSession();
                buildRequest.CodingSessionId = session.SessionId;
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

        private async void RemoteCodeSessionMgr_AppStateChanged(RemoteCodeSession session, RemoteExecutionState state)
        {
            await Clients.Caller.SendAsync("ApplicationStateChanged", session.SessionId, state);
        }

    }
}
