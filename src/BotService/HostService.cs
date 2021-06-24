using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BotService
{
    public class HostService : WebHostService
    {
        private readonly ILogger logger;

        public HostService(IWebHost host) : base(host)
        {
            this.logger = host.Services
                .GetRequiredService<ILogger<HostService>>();
        }

        protected override void OnStarting(string[] args)
        {
            logger.LogInformation("OnStarting method called.");
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            logger.LogInformation("OnStarted method called.");
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            logger.LogInformation("OnStopping method called.");
            base.OnStopping();
        }
    }
}
