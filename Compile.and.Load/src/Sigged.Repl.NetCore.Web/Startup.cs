using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sigged.Repl.NetCore.Web.Extensions;
using Sigged.Repl.NetCore.Web.Jobs;
using Sigged.Repl.NetCore.Web.Services;
using Sigged.Repl.NetCore.Web.Sockets;

namespace Sigged.Repl.NetCore.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //add httpcontext middleware so it can be injected
            services.AddHttpContextAccessor();

            //add websocket server
            services.AddSignalR(hubOptions => {
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(2);     //default 15s
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(4); //default 30s, recommended the double of the keepalive
            });

            //custom services
            services.AddTransient<IClientService, SignalRClientService>();
            services.AddSingleton<RemoteCodeSessionManager>();
            services.AddTransient<SessionCleanup>();
            services.AddQuartz(typeof(SessionCleanup));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //store local listening addresses 
            //var addressFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            //ServerInfoService.LocalAddresses = addressFeature?.Addresses;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSignalR((configure) => {
                configure.MapHub<CodeHub>("/codeHub");
            });

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseQuartz();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
