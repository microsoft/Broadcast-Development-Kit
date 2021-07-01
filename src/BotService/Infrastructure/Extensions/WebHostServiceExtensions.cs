// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;

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
