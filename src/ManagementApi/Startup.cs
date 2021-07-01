using System.Net.Http;
using System.Reflection;
using Application;
using Application.Common.Config;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using BotService.Infrastructure.Common;
using FluentValidation;
using Infrastructure.Core.Common;
using Infrastructure.Core.CosmosDbData.Extensions;
using Infrastructure.Core.CosmosDbData.Repository;
using Infrastructure.Core.Services;
using ManagementApi.Filters;
using MediatR;
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
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());

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
            services.AddTransient<IGraphServiceClient, GraphServiceClient>();
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
                        policyBuilder.RequireClaim("groups", appConfiguration.AzureAdConfiguration.GroupId);
                    });
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostEnvironment env)
        {
            if (env.IsLocal())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(_myAllowSpecificOrigins);
            }

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
                    endpoints.MapControllers().WithMetadata(new AuthorizeAttribute("Producer"));
                }
            });
        }
    }
}
