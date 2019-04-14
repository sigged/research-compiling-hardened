using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Sigged.CodeHost.Core.Dto;
using Sigged.Compiling.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sigged.CodeHost.Worker
{
    class WorkerClient
    {
        Compiler compiler;
        HubConnection connection;
        string sessionId;

        public WorkerClient(string workerHubUrl, string sessionid, Compiler compilerCore)
        {
            compiler = compilerCore;
            sessionId = sessionid;
            connection = new HubConnectionBuilder()
                .WithUrl(workerHubUrl)
                .Build();

            
        }

        public async Task Connect()
        {
            await connection.StartAsync();

            connection.On<BuildRequestDto>("Build", async (buildrequest) =>
            {
                EmitResult results = null;
                byte[] assembly = null;
                using (var stream = new MemoryStream())
                {
                    results = compiler.Compile(buildrequest.SourceCode, buildrequest.SessionId, stream,
                        outputKind: OutputKind.ConsoleApplication).Result;

                    assembly = stream.ToArray();
                }

                Console.WriteLine("CLIENT: built source code");

                BuildResultDto result = new BuildResultDto();
                result.SessionId = buildrequest.SessionId;
                result.BuildErrors = results.Diagnostics.Select(d =>
                    new BuildErrorDto
                    {
                        Id = d.Id,
                        Severity = d.Severity.ToString()?.ToLower(),
                        Description = d.GetMessage(),
                        StartPosition = LinePositionDto.FromLinePosition(d.Location.GetLineSpan().StartLinePosition),
                        EndPosition = LinePositionDto.FromLinePosition(d.Location.GetLineSpan().EndLinePosition),
                    }).ToList();

                result.IsSuccess = results.Success;

                await connection.SendAsync("BuildComplete", result);
            });

            //notify that this worker is ready to receive build requests
            await connection.SendAsync("WorkerReady", new ExecutionStateDto
            {
                SessionId = sessionId,
                State = RemoteAppState.NotRunning
            });
        }
    }
}
