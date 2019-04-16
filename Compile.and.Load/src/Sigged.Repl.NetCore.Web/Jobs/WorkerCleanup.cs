using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Jobs
{
    public class WorkerCleanup : IJob
    {
        private readonly IConfiguration configuration;
        private readonly string baseUri;
        private bool canRun;

        public WorkerCleanup(IConfiguration configuration)
        {
            this.configuration = configuration;
            baseUri = this.configuration.GetSection("ApiBaseUri").Value;
            canRun = true;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (canRun)
            {
                canRun = false;
                await Task.Delay(0);
                Console.WriteLine("Jobs - WorkerCleanup: Executing");
            }
        }

    }
}
