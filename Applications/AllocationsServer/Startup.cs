﻿using System;
using System.Net.Http;
using Allocations;
using AuthDisabler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pivotal.Discovery.Client;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Common.Discovery;
using Steeltoe.Security.Authentication.CloudFoundry;


namespace AllocationsServer
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
            // Add framework services.
            services.AddMvc();

            services.AddDbContext<AllocationContext>(options => options.UseMySql(Configuration));
            services.AddScoped<IAllocationDataGateway, AllocationDataGateway>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IProjectClient>(sp =>
            {
                var handler = new DiscoveryHttpClientHandler(sp.GetService<IDiscoveryClient>());
                var httpClient = new HttpClient(handler, false)
                {
                    BaseAddress = new Uri(Configuration.GetValue<string>("REGISTRATION_SERVER_ENDPOINT"))
                };

                  var logger = sp.GetService<ILogger<ProjectClient>>();
                 var contextAccessor = sp.GetService<IHttpContextAccessor>();
                 
                 return new ProjectClient(
                    httpClient, logger,
                    () => contextAccessor.HttpContext.GetTokenAsync("access_token")
                );
            });

            services.AddDiscoveryClient(Configuration);
            services.AddHystrixMetricsStream(Configuration);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddCloudFoundryJwtBearer(Configuration);

            if (Configuration.GetValue("DISABLE_AUTH", false))
            {
                services.DisableClaimsVerification();
            }

                 services.AddAuthorization(options =>
                 options.AddPolicy("pal-tracker", policy => policy.RequireClaim("scope", "uaa.resource")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseAuthentication();
            app.UseMvc();
            app.UseDiscoveryClient();
            app.UseHystrixMetricsStream();
            app.UseHystrixRequestContext();
        }
    }
}