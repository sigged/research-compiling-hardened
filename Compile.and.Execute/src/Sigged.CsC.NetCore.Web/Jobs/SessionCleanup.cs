using Microsoft.Extensions.Configuration;
using Quartz;
using Sigged.CsC.NetCore.Web.Services;
using System;
using System.Threading.Tasks;

namespace Sigged.CsC.NetCore.Web.Jobs
{
    public class SessionCleanup : IJob
    {
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;

        private static bool canRun = false;

        public static void Enable()
        {
            Console.WriteLine($"Jobs - SessionCleanup: signalled for execution");
            canRun = true;
        }

        public SessionCleanup(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (canRun) //postponed until first browser->hub connection! (httpcontext issue)
            {
                await Task.Delay(0);
                Console.WriteLine("Jobs - SessionCleanup: Executing");
                RemoteCodeSessionManager rcsm = null;
                try
                {
                    rcsm = serviceProvider.GetService(typeof(RemoteCodeSessionManager)) as RemoteCodeSessionManager;
                    rcsm?.CleanupIdleSessions();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Jobs - SessionCleanup: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Jobs - SessionCleanup: Skipping until signal");
            }
        }

    }
}
