// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using BotService.Infrastructure.Core;

namespace BotService.Application.Core
{
    public class MediaInjectionSettings
    {
        public string CallId { get; set; }

        public string StreamId { get; set; }

        public StreamVolume StreamVolume { get; set; }

        public ProtocolSettings ProtocolSettings { get; set; }
    }
}
