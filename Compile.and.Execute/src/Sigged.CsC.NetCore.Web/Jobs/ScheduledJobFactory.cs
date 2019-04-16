using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Concurrent;

namespace Sigged.CsC.NetCore.Web.Jobs
{
    public class ScheduledJobFactory : IJobFactory
    {
        protected readonly IServiceProvider ServiceProvider;
        private readonly ConcurrentDictionary<IJob, IServiceScope> _scopes = new ConcurrentDictionary<IJob, IServiceScope>();

        public ScheduledJobFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        // instantiation of new job
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = ServiceProvider.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            _scopes.TryAdd(job, scope);

            return job;
        }

        // executes when job is complete
        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();

            if (_scopes.TryRemove(job, out IServiceScope scope))
                scope.Dispose();
        }
    }
}
