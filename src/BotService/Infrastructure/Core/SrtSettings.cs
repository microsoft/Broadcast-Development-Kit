// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using BotService.Application.Core;
using Domain.Enums;

namespace BotService.Infrastructure.Core
{
    public class SrtSettings : ProtocolSettings
    {
        public SrtSettings()
        {
            Type = Protocol.SRT;
        }

        public string Url { get; set; }

        public int Port { get; set; }

        public string Passphrase { get; set; }

        public int Latency { get; set; }

        public SrtMode Mode { get; set; }
    }
}
