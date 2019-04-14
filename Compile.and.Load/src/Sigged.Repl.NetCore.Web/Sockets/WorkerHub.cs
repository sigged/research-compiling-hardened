using Microsoft.AspNetCore.SignalR;
using Sigged.CodeHost.Core.Dto;
using Sigged.Repl.NetCore.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Sockets
{
    public class WorkerHub : Hub
    {
        private RemoteCodeSessionManager _rcsm;

        public WorkerHub(RemoteCodeSessionManager rcsm)
        {
            _rcsm = rcsm;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override Task OnConnectedAsync()
        {
            var caller = Clients.Caller;
            var remoteIp = Context.GetHttpContext().Connection.RemoteIpAddress;
            if (!IPAddress.IsLoopback(remoteIp))
            {
                Context.Abort();
            }
            return base.OnConnectedAsync();
        }

        public async Task WorkerReady(ExecutionStateDto executionState)
        {
            string sessionid = executionState.SessionId;

            await Clients.Caller.SendAsync("Build", new BuildRequestDto
            {
                SessionId = sessionid,
                RunOnSuccess = true,
                SourceCode = @"
/* C# demo code */
using System;

namespace Test {

    public class Program {
    
        public static void Main(string[] args) 
        {
            Console.Write(""What is your\nname ? "");
            //char input = (char)Console.Read();
            string input = Console.ReadLine();
            //Console.WriteLine($""Hello { input }"");
            Console.WriteLine($""Nice to meet you"");
            //int i = 0, j = 1;
            //i = j / i;
        }
    }
}
                    "
            });
        }

        public async Task BuildComplete(BuildResultDto buildResult)
        {
            if (buildResult.IsSuccess)
            {
                
            }
        }


    }
}
