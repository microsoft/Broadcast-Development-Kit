// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Reflection;
using Application;
using Application.Common.Config;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using BotService.Application.Core;
using BotService.Configuration;
using BotService.Infrastructure.Client;
using BotService.Infrastructure.Common;
using BotService.Infrastructure.Common.Logging;
using BotService.Infrastructure.Core;
using BotService.Infrastructure.Pipelines;
using BotService.Infrastructure.Services;
using FluentValidation;
using Infrastructure.Core.Common;
using Infrastructure.Core.CosmosDbData.Extensions;
using Infrastructure.Core.CosmosDbData.Repository;
using Infrastructure.Core.Services;
using MediatR;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Communications.Client;
using Microsoft.Graph.Communications.Common.Telemetry;

namespace BotService
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        public Startup(
            IConfiguration configuration,
            ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var environment = new HostEnvironment();
            services.AddSingleton<IHostEnvironment>(environment);

            var appConfiguration = Configuration.GetSection("Settings").Get<AppConfiguration>();
            services.AddSingleton<IAppConfiguration>(appConfiguration);

            var graphLogger = new GraphLogger(typeof(Program).Assembly.GetName().Name, redirectToTrace: true);
            services.AddSingleton<IGraphLogger>(graphLogger);

            var sampleObserver = new SampleObserver(graphLogger);
            services.AddSingleton(sampleObserver);
            services.AddSingleton<PipelineBusObserver>();

            var clientBuilder = new GraphCommunicationsClientBuilder(appConfiguration, graphLogger, _logger);
            var communicationClient = clientBuilder.Build();
            services.AddSingleton<ICommunicationsClient>(communicationClient);

            if (!environment.IsLocal())
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                {
                    options.Authority = $"{appConfiguration.AzureAdConfiguration.Instance}{appConfiguration.AzureAdConfiguration.TenantId}/v2.0";
                    options.Audience = $"{appConfiguration.BotServiceAuthenticationConfiguration.BotServiceApiClientId}";
                });
            }

            services.AddApplication();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // register CosmosDB client and data repositories
            services.AddCosmosDb(
                appConfiguration.CosmosDbConfiguration.EndpointUrl,
                appConfiguration.CosmosDbConfiguration.PrimaryKey,
                appConfiguration.CosmosDbConfiguration.DatabaseName);

            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<ICallRepository, CallRepository>();
            services.AddScoped<IParticipantStreamRepository, ParticipantStreamRepository>();
            services.AddScoped<IStreamRepository, StreamRepository>();
            services.AddScoped<IAzStorageHandler, AzStorageHandler>();

            services.AddSingleton<IMediatorService, MediatorService>();
            services.AddSingleton<IMediaProcessorFactory, GStreamerMediaProcessorFactory>();
            services.AddSingleton<IMediaHandlerFactory, MediaHandlerFactory>();
            services.AddSingleton<IBot, Bot>();

            services.AddApplicationInsightsTelemetry();

            services.AddSingleton<ITelemetryInitializer, CloudRoleNameTelemetryInitializer>();

            services.AddScoped<IInjectionUrlHelper, InjectionUrlHelper>();

            services.AddMvc(config =>
            {
                if (!environment.IsLocal())
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireRole("BotService.AccessAll")
                        .Build();

                    config.Filters.Add(new AuthorizeFilter(policy));
                }
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostEnvironment env,
            IBot bot,
            IAppConfiguration appConfiguration)
        {
            if (env.IsProduction())
            {
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.ConfigureExceptionHandler(_logger, env.IsProduction(), "BotServiceApi");
            app.UseMvc();
            bot.RegisterServiceAsync(appConfiguration.BotConfiguration.VirtualMachineName).Wait();
        }
    }
}
