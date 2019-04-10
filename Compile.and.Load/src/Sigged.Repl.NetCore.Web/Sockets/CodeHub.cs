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
        private RemoteCodeSessionManager remoteCodeSessionMgr;

        public CodeHub(RemoteCodeSessionManager rcsm)
        {
            remoteCodeSessionMgr = rcsm;
        }

        private async Task<BuildResultDto> BuildCore(BuildRequestDto buildRequest)
        {
            BuildResultDto result = new BuildResultDto();
            EmitResult results = await remoteCodeSessionMgr.Compile(buildRequest.CodingSessionId, buildRequest.SourceCode);
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

        private async Task<bool> RunCore(BuildRequestDto buildRequest)
        {
            return await remoteCodeSessionMgr.RunLastCompilation(buildRequest.CodingSessionId);
        }

        public async Task Build(BuildRequestDto buildRequest)
        {
            var session = remoteCodeSessionMgr.GetSession(buildRequest.CodingSessionId);
            if (session == null)
            {
                session = remoteCodeSessionMgr.CreateSession();
                buildRequest.CodingSessionId = session.SessionId;
            }
            BuildResultDto result = await BuildCore(buildRequest);
            await Clients.Caller.SendAsync("BuildComplete", result);
        }

        public async Task RunCode(BuildRequestDto buildRequest)
        {
            var session = remoteCodeSessionMgr.GetSession(buildRequest.CodingSessionId);
            if (session == null)
            {
                session = remoteCodeSessionMgr.CreateSession();
                buildRequest.CodingSessionId = session.SessionId;

                if(session.LastResult == null || session.LastAssembly == null)
                {
                    //should compile first
                    BuildResultDto result = await BuildCore(buildRequest);
                    await Clients.Caller.SendAsync("BuildComplete", result);
                }
                
            }

            if (session.LastResult.Success)
            {
                //run code
                bool ok = await RunCore(buildRequest);
                await Clients.Caller.SendAsync("ApplicationRunning", ok);
            }
            else
            {
                await Clients.Caller.SendAsync("ApplicationRunning", false);
            }
        }
    }
}
