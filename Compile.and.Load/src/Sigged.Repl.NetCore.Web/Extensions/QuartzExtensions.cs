using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Sigged.Repl.NetCore.Web.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Extensions
{
    public static class QuartzExtensions
    {
        public static string WorkerCleanupJob = "WorkerCleanupJob.job";
        public static string MaintenanceJobsGroup = "MaintenanceJobs";
        public static string MaintenanceJobsTrigger = "MaintenanceJobs.trigger";
        public static int WorkerCleanupJobInterval = 20; //sconds

        public static void AddQuartz(this IServiceCollection services, Type jobType)
        {
            services.Add(new ServiceDescriptor(typeof(IJob), jobType, ServiceLifetime.Transient));
            services.AddSingleton<IJobFactory, ScheduledJobFactory>();

            services.AddSingleton<IJobDetail>(provider => JobBuilder.Create<WorkerCleanup>()
                    .WithIdentity(WorkerCleanupJob, MaintenanceJobsGroup)
                    .Build());

            services.AddSingleton<ITrigger>(provider =>
            {
                return TriggerBuilder.Create()
                    .WithIdentity(MaintenanceJobsTrigger, MaintenanceJobsGroup)
                    .StartNow()
                    .WithSimpleSchedule
                    (s =>
                        s.WithInterval(TimeSpan.FromSeconds(WorkerCleanupJobInterval))
                            .RepeatForever()
                    )
                    .Build();
            });

            services.AddSingleton<IScheduler>(provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = schedulerFactory.GetScheduler().Result;
                scheduler.JobFactory = provider.GetService<IJobFactory>();
                scheduler.Start();
                return scheduler;
            });

        }

        public static void UseQuartz(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<IScheduler>()
                .ScheduleJob(app.ApplicationServices.GetService<IJobDetail>(),
                    app.ApplicationServices.GetService<ITrigger>()
                );
        }
    }
}
