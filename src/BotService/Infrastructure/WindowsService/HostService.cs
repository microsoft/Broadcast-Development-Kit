// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Runtime.InteropServices;
using BotService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BotService.Infrastructure.WindowsService
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
            _logger.LogInformation("Starting the bot service");

            // At this point, all dependencies have been registered and the configuration was retrieved
            SetServiceAsStartPending(2);

            _webHost.SetupDatabase();
            SetServiceAsStartPending(3);

            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            // At this point, the ASP.NET host should be running and receiving requests
            SetServiceAsStartPending(4);

            _webHost.RegisterBotService();
            SetServiceAsRunning();

            _logger.LogInformation("The bot service completed the start up process.");

            base.OnStarted();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("Stopping the bot service.");
            _webHost.UnregisterBotService();

            base.OnStopping();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("The bot service was stopped.");

            base.OnStopped();
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref WindowsServiceStatus serviceStatus);

        private void SetServiceAsStartPending(int? progress = null)
        {
            var serviceStatus = default(WindowsServiceStatus);
            serviceStatus.dwCurrentState = WindowsServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000; // 100 seconds

            if (progress.HasValue)
            {
                // The number itself doesn't have any meaning, but it needs to be increased in each call to this method
                // to inform Windows that the service is making progress and didn't hang-up.
                serviceStatus.dwCheckPoint = progress.Value;
            }

            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        private void SetServiceAsRunning()
        {
            var serviceStatus = default(WindowsServiceStatus);
            serviceStatus.dwCurrentState = WindowsServiceState.SERVICE_RUNNING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }
    }
}
