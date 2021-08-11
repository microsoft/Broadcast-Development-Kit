// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading.Tasks;
using BotService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            _webHost.SetupAndRegisterBotService();

            base.OnStarting(args);
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping method called.");
            _webHost.UnregisterBotService();
            base.OnStopping();
        }
    }
}
