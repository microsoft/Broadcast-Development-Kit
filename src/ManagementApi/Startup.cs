// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Net.Http;
using Application;
using Application.Common.Config;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using BotService.Infrastructure.Common;
using Infrastructure.Core.Common;
using Infrastructure.Core.CosmosDbData.Extensions;
using Infrastructure.Core.CosmosDbData.Repository;
using Infrastructure.Core.Services;
using ManagementApi.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace ManagementApi
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        private readonly string _myAllowSpecificOrigins = "MyOrigins";

        public Startup(
            IWebHostEnvironment environment,
            ILogger<Startup> logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName.ToLowerInvariant()}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var hostEnvironment = new HostEnvironment();
            services.AddSingleton<IHostEnvironment>(hostEnvironment);

            /*
                TODO: This is a dirty workaround. We should modify the BotServiceClient, and change the protocol to HTTP
                if the running environment is local.
             */
            if (hostEnvironment.IsLocal())
            {
                services.AddHttpClient("bot-service-client")
                   .ConfigurePrimaryHttpMessageHandler(() =>
                   {
                       var clientHandler = new HttpClientHandler()
                       {
                           AllowAutoRedirect = false,
                           UseDefaultCredentials = true,
#pragma warning disable S4830 // This configuration is only applied when the solution is configured to run locally to simply the configuration.
                           ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
#pragma warning restore S4830
                           {
                               return true;
                           },
                       };

                       return clientHandler;
                   });
            }
            else
            {
                services.AddHttpClient("bot-service-client");
            }

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(AddBuildVersionHeaderFilter));
            }).AddNewtonsoftJson();

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddApplicationInsightsTelemetry();

            var appConfiguration = Configuration.GetSection("Settings").Get<AppConfiguration>();
            services.AddSingleton<IAppConfiguration>(appConfiguration);

            services.AddApplication();

            // register CosmosDB client and data repositories
            services.AddCosmosDb(
                appConfiguration.CosmosDbConfiguration.EndpointUrl,
                appConfiguration.CosmosDbConfiguration.PrimaryKey,
                appConfiguration.CosmosDbConfiguration.DatabaseName);

            services.AddScoped<ICallRepository, CallRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IParticipantStreamRepository, ParticipantStreamRepository>();
            services.AddScoped<IStreamRepository, StreamRepository>();

            services.AddScoped<IAzStorageHandler, AzStorageHandler>();
            services.AddScoped<IBotServiceClient, BotServiceClient>();
            services.AddSingleton<IBotServiceAuthenticationProvider, BotServiceAuthenticationProvider>();

            services.AddSingleton<IAzService, AzService>();
            services.AddSingleton<IAzure>(serviceProvider =>
            {
                var azService = serviceProvider.GetService<IAzService>();
                var azure = azService.GetAzure();

                return azure;
            });
            services.AddScoped<IAzVirtualMachineService, AzVirtualMachineService>();

            services.AddTransient<IAuthenticationProvider, GraphAuthenticationProvider>();
            services.AddTransient<GraphServiceClient>();
            services.AddTransient<IGraphService, MicrosoftGraphService>();

            services.AddTransient<IMeetingUrlHelper, MeetingUrlHelper>();
            services.AddTransient<IStreamKeyGeneratorHelper, StreamKeyGeneratorHelper>();

            if (hostEnvironment.IsLocal())
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(
                        name: _myAllowSpecificOrigins,
                        builder =>
                        {
                            builder
                                .AllowAnyOrigin()
                                .WithHeaders(HeaderNames.Authorization, HeaderNames.ContentType, "x-client")
                                .AllowAnyMethod();
                        });
                });
            }
            else
            {
                var azureAdConfiguration = Configuration.GetSection("Settings").GetSection(nameof(AzureAdConfiguration));

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddMicrosoftIdentityWebApi(azureAdConfiguration)
                        .EnableTokenAcquisitionToCallDownstreamApi()
                        .AddInMemoryTokenCaches();

                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Producer", policyBuilder =>
                    {
                        policyBuilder.RequireAssertion(authorizationHandlerContext =>
                        {
                            return authorizationHandlerContext.User.HasClaim("groups", appConfiguration.AzureAdConfiguration.GroupId) || authorizationHandlerContext.User.IsInRole("ManagementAPI.AccessAll");
                        });
                    });
                });
            }

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BDK Management API",
                    Version = "v1",
                    Description = "API to support operations to extract and inject media streams from the meeting (e.g. participants, screen share, etc.) and use them as sources for producing live content.",
                });

                options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme.",
                });
                var secScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" },
                };
                var secReq = new OpenApiSecurityRequirement();
                secReq.Add(secScheme, new string[] { });
                options.AddSecurityRequirement(secReq);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostEnvironment env,
            IAppConfiguration appConfiguration)
        {
            if (env.IsLocal())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(_myAllowSpecificOrigins);
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.ConfigureExceptionHandler(_logger, env.IsProduction());
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                if (env.IsLocal())
                {
                    endpoints.MapControllers();
                }
                else
                {
                    if (string.IsNullOrEmpty(appConfiguration.AzureAdConfiguration.GroupId))
                    {
                        endpoints.MapControllers().WithMetadata(new AuthorizeAttribute());
                    }
                    else
                    {
                        endpoints.MapControllers().WithMetadata(new AuthorizeAttribute("Producer"));
                    }
                }
            });
        }
    }
}
