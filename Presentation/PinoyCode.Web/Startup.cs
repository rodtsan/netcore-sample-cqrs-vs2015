﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using PinoyCode.Cqrs;
using PinoyCode.Data.Infrustracture;
using PinoyCode.Domain.Ads;
using PinoyCode.Domain.Ads.Repositories;
using PinoyCode.Domain.Identity;
using PinoyCode.Domain.Identity.Handlers;
using PinoyCode.Domain.Identity.Models;
using PinoyCode.Web.Controllers;
using PinoyCode.Web.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PinoyCode.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddEntityFrameworkSqlServer()
                    .AddDbContext<AdsDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                     c => c.MigrationsAssembly("PinoyCode.Web"))
                    , ServiceLifetime.Scoped);

            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
            })
                .AddEntityFrameworkStores<AdsDbContext, Guid>()
                .AddDefaultTokenProviders();

            services.AddMvc();


            // Add application services.
            services.AddTransient<IDbContext, AdsDbContext>();
            services.AddTransient<IEventStore, SqlEventStore>();
            services.AddTransient<IMessageDispatcher, MessageDispatcher>();
            services.AddTransient<IAdsBusinessObject, AdsBusinessObject>();


            services.AddTransient<IUserManager, UserManager>();
            services.AddTransient<ISignInManager, SignInManager>();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //IFileProvider fileProvider = new PhysicalFileProvider(
            //    Path.Combine(Directory.GetCurrentDirectory(), @"Static"));
            //PathString requestPath = new PathString("/classified-ads");

            //app.UseDefaultFiles(new DefaultFilesOptions()
            //{
            //    DefaultFileNames = new string[] { "default.html", "default.htm", "index.html", "index.htm" },
            //    FileProvider = fileProvider,
            //    RequestPath = requestPath
            //});

            //app.UseStaticFiles(new StaticFileOptions()
            //{ 
            //    FileProvider = fileProvider,
            //    RequestPath = requestPath
            //});


            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            //app.Run(ctx =>
            //{
            //    ctx.Response.Redirect("/classified-ads");
            //    return Task.FromResult(0);
            //});
        }
    }
}
