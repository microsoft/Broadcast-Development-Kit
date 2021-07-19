// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.ServiceProcess;
using Application.Common.Config;
using Application.Interfaces.Common;
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

        public static void SetupAndRegisterBotService(this IWebHost host)
        {
            var logger = host.Services.GetService<ILogger<IWebHost>>();

            logger.LogInformation("Setting up database schema");
            var cosmosDbSetup = host.Services.GetService<ICosmosDbSetup>();
            cosmosDbSetup.SetupDatabaseAsync().Wait();

            var appConfiguration = host.Services.GetService<IAppConfiguration>();
            var bot = host.Services.GetService<IBot>();

            bot.RegisterServiceAsync(appConfiguration.BotConfiguration.VirtualMachineName).Wait();
        }
    }
}
