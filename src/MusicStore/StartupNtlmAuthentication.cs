using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Net.Http.Server;
using MusicStore.Components;
using MusicStore.Models;

namespace MusicStore
{
    /// <summary>
    /// To make runtime to load an environment based startup class, specify the environment by the following ways:
    /// 1. Drop a Microsoft.AspNetCore.Hosting.ini file in the wwwroot folder
    /// 2. Add a setting in the ini file named 'ASPNETCORE_ENVIRONMENT' with value of the format 'Startup[EnvironmentName]'.
    ///    For example: To load a Startup class named 'StartupNtlmAuthentication' the value of the env should be
    ///    'NtlmAuthentication' (eg. ASPNETCORE_ENVIRONMENT=NtlmAuthentication). Runtime adds a 'Startup' prefix to this and
    ///    loads 'StartupNtlmAuthentication'.
    /// If no environment name is specified the default startup class loaded is 'Startup'.
    ///
    /// Alternative ways to specify environment are:
    /// 1. Set the environment variable named SET ASPNETCORE_ENVIRONMENT=NtlmAuthentication
    /// 2. For selfhost based servers pass in a command line variable named --env with this value. Eg:
    /// "commands": {
    ///    "web": "Microsoft.AspNetCore.Hosting --server Microsoft.AspNetCore.Server.WebListener
    ///           --server.urls http://localhost:5002 --ASPNETCORE_ENVIRONMENT NtlmAuthentication",
    ///  },
    /// </summary>
    public class StartupNtlmAuthentication
    {
        public StartupNtlmAuthentication(IApplicationEnvironment applicationEnvironment)
        {
            // Below code demonstrates usage of multiple configuration sources. For instance a setting say 'setting1'
            // is found in both the registered sources, then the later source will win. By this way a Local config
            // can be overridden by a different setting while deployed remotely.
            var builder = new ConfigurationBuilder()
                .SetBasePath(applicationEnvironment.ApplicationBasePath)
                .AddJsonFile("config.json")
                //All environment variables in the process's context flow in as configuration values.
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Add EF services to the services container
            services.AddDbContext<MusicStoreContext>(options =>
                            options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            // Add Identity services to the services container
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<MusicStoreContext>()
                    .AddDefaultTokenProviders();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("http://example.com");
                });
            });

            // Add MVC services to the services container
            services.AddMvc();

            // Add memory cache services
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            // Add session related services.
            services.AddSession();

            // Add the system clock service
            services.AddSingleton<ISystemClock, SystemClock>();

            // Configure Auth
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "ManageStore",
                    authBuilder => {
                        authBuilder.RequireClaim("ManageStore", "Allowed");
                    });
            });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(minLevel: LogLevel.Warning);

            app.UseStatusCodePagesWithRedirects("~/Home/StatusCodePage");

            // Error page middleware displays a nice formatted HTML page for any unhandled exceptions in the
            // request pipeline.
            // Note: Not recommended for production.
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();

            // Add the runtime information page that can be used by developers
            // to see what packages are used by the application
            // default path is: /runtimeinfo
            app.UseRuntimeInfoPage();

            app.Use(async (context, next) =>
            {
                // Who will get admin access? For demo sake I'm listing the currently logged on user as the application
                // administrator. But this can be changed to suit the needs.
                var identity = (ClaimsIdentity)context.User.Identity;

                if (context.User.Identity.Name == Environment.GetEnvironmentVariable("USERDOMAIN") + "\\"
                    + Environment.GetEnvironmentVariable("USERNAME"))
                {
                    identity.AddClaim(new Claim("ManageStore", "Allowed"));
                }

                await next.Invoke();
            });

            // Configure Session.
            app.UseSession();

            // Add static files to the request pipeline
            app.UseStaticFiles();

            // Add MVC to the request pipeline
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    name: "api",
                    template: "{controller}/{id?}");
            });

            //Populates the MusicStore sample data
            SampleData.InitializeMusicStoreDatabaseAsync(app.ApplicationServices, false).Wait();
        }
    }
}
