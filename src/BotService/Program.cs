// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using BotService.Configuration;
using BotService.Infrastructure.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace BotService
{
    public static class Program
    {
        private static IConfigurationRoot _configurationRoot;

        public static int Main(string[] args)
        {

            try
            {
                _configurationRoot = ConfigurationBuilderExtensions.GetConfiguration(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error has ocurred while trying to get the bot's configuration. Ex: {ex.Message}");
                throw;
            }

            Log.Logger = LoggerConfigurationHelper.GetLogger(_configurationRoot);
            try
            {
                Log.Information("Initializing Bot Service");
                Init(args);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return -1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static void Init(string[] args)
        {
            Log.Information("Parsing configuration");
            var appConfiguration = _configurationRoot.GetSection("Settings").Get<AppConfiguration>();

            Log.Information("Getting certificate");
            var certificate = BotCertificateHelper.GetCertificate(appConfiguration);

            Log.Information("Initializing GStreamer");
            Gst.Application.Init();

            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            Log.Information("Creating web host");
            var hostBuilder = CreateWebHostBuilder(
                args.Where(arg => arg != "--console").ToArray(),
                _configurationRoot,
                certificate);

            var host = hostBuilder.Build();

            if (isService)
            {
                Log.Information("Running web host as a service");
                host.RunAsCustomService();
            }
            else
            {
                // When running as a console app we have time to run this part of the setup before starting the web host.
                host.SetupDatabase();
                host.RegisterBotService();

                // Before running the host and blocking the calling thread, we attach the CancelKeyPress event to unregister the bot.
                Console.CancelKeyPress += (sender, e) =>
                {
                    host.UnregisterBotService();
                };

                Log.Information("Running web host as a console application");
                host.Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(
            string[] args,
            IConfigurationRoot configuration,
            X509Certificate2 certificate) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options => options.ConfigureEndpoints(certificate))
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
                .ConfigureAppConfiguration(builder =>
                {
                    builder.Sources.Clear();
                    builder.AddConfiguration(configuration);
                })
                .UseStartup<Startup>();
    }
}
