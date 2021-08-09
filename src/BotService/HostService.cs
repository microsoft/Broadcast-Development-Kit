// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using BotService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BotService
{
    public class HostService : WebHostService
    {
        private readonly IWebHost _webHost;
        private readonly ILogger _logger;

        public HostService(IWebHost host)
            : base(host)
        {
            _webHost = host;
            _logger = host.Services.GetRequiredService<ILogger<HostService>>();
        }

        protected override void OnStarting(string[] args)
        {
            _logger.LogInformation("OnStarting method called.");
            Task.Delay(30000).Wait();
            _webHost.SetupAndRegisterBotService();

            base.OnStarting(args);
        }

        protected override void OnStopped()
        {
            // TODO: Unregister service
            _logger.LogInformation("OnStopping method called.");
            _webHost.UnregisterBotService();
            base.OnStopped();
        }
    }
}
