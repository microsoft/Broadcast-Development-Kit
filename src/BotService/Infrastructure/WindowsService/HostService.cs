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
            // At this point, all dependencies have been registered and the configuration was retrieved
            SetServiceAsStartPending(2);

            _webHost.SetupDatabase();
            SetServiceAsStartPending(3);

            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            // At this point, the ASP.NET host is now running and receiving requests
            SetServiceAsStartPending(4);

            _webHost.RegisterBotService();
            SetServiceAsRunning();

            base.OnStarted();
        }

        protected override void OnStopping()
        {
            // TODO: Unregister service
            _logger.LogInformation("OnStopping method called.");
            base.OnStopping();
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
