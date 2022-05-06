﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace ManagementApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
           WebHost.CreateDefaultBuilder(args)
             .ConfigureKestrel((context, options) =>
             {
                 options.AllowSynchronousIO = true;
             })
             .UseSerilog((hostingContext, loggerConfiguration) =>
             {
                 loggerConfiguration
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.ApplicationInsights(hostingContext.Configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY"), TelemetryConverter.Traces);
             })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddAzureWebAppDiagnostics();
            })
            .UseStartup<Startup>();
    }
}
