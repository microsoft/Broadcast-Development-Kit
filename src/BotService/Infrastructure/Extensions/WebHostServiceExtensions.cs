using Microsoft.AspNetCore.Hosting;
using System.ServiceProcess;

namespace BotService.Infrastructure.Extensions
{
    public static class WebHostServiceExtensions
    {
        public static void RunAsCustomService(this IWebHost host)
        {
            var webHostService = new HostService(host);
            ServiceBase.Run(webHostService);
        }
    }
}
