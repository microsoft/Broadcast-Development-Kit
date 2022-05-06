// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace BotService.Infrastructure.Extensions
{
    public static class LoggerConfigurationHelper
    {
        private const string AppInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";

        public static ILogger GetLogger(IConfigurationRoot configuration)
        {
            var appInsightsKey = configuration[AppInsightsInstrumentationKey];

            var defaultLoggerConfiguration = new LoggerConfiguration()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.Console();

            if (!string.IsNullOrEmpty(appInsightsKey))
            {
                var telemetryConfiguration = TelemetryConfiguration
                    .CreateDefault();
                telemetryConfiguration.InstrumentationKey = appInsightsKey;
                defaultLoggerConfiguration.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
            }

            var logger = defaultLoggerConfiguration.CreateLogger();
            return logger;
        }

        public static ILogger GetConsoleLogger()
        {
            var logger = new LoggerConfiguration()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .CreateLogger();

            return logger;
        }
    }
}
