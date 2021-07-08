// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace BotService.Configuration
{
    public class EndpointConfiguration
    {
        public string Host { get; set; }

        public int? Port { get; set; }

        public string Scheme { get; set; }
    }
}
