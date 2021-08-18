// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Application.Common.Config;
using Application.Exceptions;
using BotService.Configuration;
using BotService.Infrastructure.Extensions;
using Infrastructure.Core.Common.Extensions;
using Infrastructure.Core.Services;
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
        private const string EnvVariableKey = "BROADCASTER_ENVIRONMENT";
        private const string BlobSasQueryEnvVariableKey = "BLOB_SAS_QUERY";
        private const string StorageAcountEnvVariableKey = "STORAGE_ACCOUNT";
        private const string BlobContainerEnvVariableKey = "BLOB_CONTAINER";
        private const string AppSettingsNameEnvVariableKey = "APP_SETTINGS_FILE_NAME";
        private const string CertificateNameEnvVariableKey = "CERTIFICATE_FILE_NAME";
        private const string DefaultAppSettingsFileName = "appsettings.json";
        private const string DefaultCertificateFileName = "certificate.pfx";
        private const string LocalEnvironment = "local";

        private static IConfigurationRoot _configuration;
        private static X509Certificate2 _certificate;

        public static async Task Main(string[] args)
        {
            await Init(args);

            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            var builder = CreateWebHostBuilder(
                args.Where(arg => arg != "--console").ToArray());

            var host = builder.Build();

            Console.WriteLine($"Is a Service {isService}");

            if (isService)
            {
                host.RunAsCustomService();
            }
            else
            {
                // When running as a console app we have time to run this part of the setup before starting the web host.
                host.SetupDatabase();
                host.RegisterBotService();
                host.Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options => options.ConfigureEndpoints(_certificate))
                .UseConfiguration(_configuration)
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

        private static async Task Init(string[] args)
        {
            Gst.Application.Init();
            var environment = Environment.GetEnvironmentVariable(EnvVariableKey);
            var builder = new ConfigurationBuilder().AddCommandLine(args);

            if (string.Equals(environment, LocalEnvironment, StringComparison.InvariantCultureIgnoreCase))
            {
                SetLocalConfiguration(builder);
            }
            else
            {
                await SetCloudConfiguration(builder);
            }
        }

        private static void SetLocalConfiguration(IConfigurationBuilder builder)
        {
            builder.AddJsonFile($"appsettings.{LocalEnvironment}.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();
            var appConfiguration = _configuration.GetSection("Settings").Get<AppConfiguration>();
            _certificate = GetCertificateFromStore(appConfiguration.BotConfiguration.CertificateThumbprint);
        }

        private static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: true);
                if (certs.Count != 1)
                {
                    throw new CertificateNotFoundException($"No certificate with thumbprint {thumbprint} was found in the machine store.");
                }

                return certs[0];
            }
            finally
            {
                store.Close();
            }
        }

        private static async Task SetCloudConfiguration(IConfigurationBuilder builder)
        {
            var cloudConfigurationService = GetCloudConfigurationService();
            var appSettingsStream = await cloudConfigurationService.GetAppSettingsAsStreamAsync();

            builder.AddJsonStream(appSettingsStream);
            _configuration = builder.Build();
            var appConfiguration = _configuration.GetSection("Settings").Get<AppConfiguration>();

            _certificate = await InstallCertificateIfMissing(appConfiguration, cloudConfigurationService);
        }

        private static CloudConfigurationService GetCloudConfigurationService()
        {
            var appSettingsFileName = Environment.GetEnvironmentVariable(AppSettingsNameEnvVariableKey);
            var certificateFileName = Environment.GetEnvironmentVariable(CertificateNameEnvVariableKey);

            var cloudConfigSettings = new CloudConfigSettings()
            {
                AppSettingsFileName = string.IsNullOrEmpty(appSettingsFileName) ? DefaultAppSettingsFileName : appSettingsFileName,
                BlobContainerName = Environment.GetEnvironmentVariable(BlobContainerEnvVariableKey),
                CertificateFileName = string.IsNullOrEmpty(certificateFileName) ? DefaultCertificateFileName : certificateFileName,
                SasToken = Environment.GetEnvironmentVariable(BlobSasQueryEnvVariableKey),
                StorageAccountName = Environment.GetEnvironmentVariable(StorageAcountEnvVariableKey),
            };

            var cloudConfigurationService = new CloudConfigurationService(cloudConfigSettings);

            return cloudConfigurationService;
        }

        private static async Task<X509Certificate2> InstallCertificateIfMissing(AppConfiguration appConfiguration, CloudConfigurationService cloudConfigurationService)
        {
            try
            {
                return GetCertificateFromStore(appConfiguration.BotConfiguration.CertificateThumbprint);
            }
            catch (CertificateNotFoundException)
            {
                // TODO: Send this log to application insights
                Console.WriteLine("No certificate was found in the machine. The service will attempt to download the certificate and install it in the machine.");
            }

            // We didn't found the certificate, so we will try to download it and install it in the machine.
            var certificateStream = await cloudConfigurationService.GetCertificateAsync();

            return InstallCertificate(certificateStream, appConfiguration.BotConfiguration.CertificatePassword);
        }

        private static X509Certificate2 InstallCertificate(Stream certificateStream, string password)
        {
            var certBytes = certificateStream.ToByteArray();
            var cert = new X509Certificate2(certBytes, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();

            return cert;
        }
    }
}
