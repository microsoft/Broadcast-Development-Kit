using Application;
using Application.Common.Config;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using FluentValidation;
using Infrastructure.Core.Common;
using Infrastructure.Core.CosmosDbData.Extensions;
using Infrastructure.Core.CosmosDbData.Repository;
using Infrastructure.Core.Services;
using MediatR;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

[assembly: FunctionsStartup(typeof(BotOrchestrator.Startup))]
namespace BotOrchestrator
{
    public class Startup: FunctionsStartup
    {
        private IConfigurationRoot _configuration;

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {

            _configuration = builder.ConfigurationBuilder.Build(); 
        }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var appConfiguration = new FunctionAppConfiguration(_configuration);
            builder.Services.AddSingleton<IAppConfiguration>(appConfiguration);

            var hostEnvironment = new HostEnvironment(isAzureFunction: true);
            builder.Services.AddSingleton<IHostEnvironment>(hostEnvironment);

            builder.Services.AddCosmosDb(appConfiguration.CosmosDbConfiguration.EndpointUrl,
                                   appConfiguration.CosmosDbConfiguration.PrimaryKey,
                                   appConfiguration.CosmosDbConfiguration.DatabaseName,
                                   hostEnvironment);

            builder.Services.AddScoped<ICallRepository, CallRepository>();
            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            builder.Services.AddSingleton<IBotServiceAuthenticationProvider, BotServiceAuthenticationProvider>();

            Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .WriteTo.Console(theme: AnsiConsoleTheme.Code)
              .WriteTo.ApplicationInsights(FunctionAppConfiguration.ApplicationInsightsKey, TelemetryConverter.Traces)
              .CreateLogger();

            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            builder.Services.AddLogging(x => x.SetMinimumLevel(LogLevel.Debug));

            builder.Services.AddApplication();
            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

            builder.Services.AddTransient<IAzStorageHandler, AzStorageHandler>();

            builder.Services.AddSingleton<IAzService, AzService>();
            builder.Services.AddSingleton<IAzure>(serviceProvider =>
            {
                var azService = serviceProvider.GetService<IAzService>();
                var azure = azService.GetAzure();

                return azure;
            });

            builder.Services.AddTransient<IAzVirtualMachineService, AzVirtualMachineService>();

            builder.Services.AddTransient<IBotServiceClient, BotServiceClient>();

            builder.Services.AddHttpClient();
        }
    }
}
