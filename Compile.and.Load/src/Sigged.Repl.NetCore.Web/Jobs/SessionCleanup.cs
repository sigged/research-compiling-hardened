using Microsoft.Extensions.Configuration;
using Quartz;
using Sigged.Repl.NetCore.Web.Services;
using System;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Jobs
{
    public class SessionCleanup : IJob
    {
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;
        private readonly string baseUri;
        private bool canRun;

        public SessionCleanup(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            baseUri = this.configuration.GetSection("ApiBaseUri").Value;
            canRun = true;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (canRun)
            {
                canRun = false;
                await Task.Delay(0);
                Console.WriteLine("Jobs - SessionCleanup: Executing");
                RemoteCodeSessionManager rcsm = null;
                try
                {
                    rcsm = serviceProvider.GetService(typeof(RemoteCodeSessionManager)) as RemoteCodeSessionManager;   
                }
                catch
                {
                    //will happen when app starts and IHttpAccessor cannot be resolved yet
                    Console.WriteLine("Jobs - SessionCleanup: error resolving a dependency");
                    throw;
                }
                rcsm?.CleanupIdleSessions();
            }
        }

    }
}
