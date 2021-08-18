// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.ServiceProcess;
using Application.Common.Config;
using Application.Interfaces.Common;
using BotService.Infrastructure.WindowsService;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BotService.Infrastructure.Extensions
{
    public static class WebHostServiceExtensions
    {
        public static void RunAsCustomService(this IWebHost host)
        {
            var webHostService = new HostService(host);
            ServiceBase.Run(webHostService);
        }

        public static void SetupDatabase(this IWebHost host)
        {
            var logger = host.Services.GetService<ILogger<IWebHost>>();

            logger.LogInformation("Setting up database schema");
            var cosmosDbSetup = host.Services.GetService<ICosmosDbSetup>();
            cosmosDbSetup.SetupDatabaseAsync().Wait();
        }

        public static void RegisterBotService(this IWebHost host)
        {
            var logger = host.Services.GetService<ILogger<IWebHost>>();

            logger.LogInformation("Registering bot service");
            var appConfiguration = host.Services.GetService<IAppConfiguration>();
            var bot = host.Services.GetService<IBot>();

            bot.RegisterServiceAsync(appConfiguration.BotConfiguration.VirtualMachineName).Wait();
        }

        public static void UnregisterBotService(this IWebHost host)
        {
            var logger = host.Services.GetService<ILogger<IWebHost>>();

            logger.LogInformation("Unregistering bot service");

            var appConfiguration = host.Services.GetService<IAppConfiguration>();
            var bot = host.Services.GetService<IBot>();

            bot.UnregisterServiceAsync(appConfiguration.BotConfiguration.VirtualMachineName).Wait();
        }
    }
}
