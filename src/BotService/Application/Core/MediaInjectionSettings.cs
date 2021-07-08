// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace BotService.Application.Core
{
    public class MediaInjectionSettings
    {
        public string CallId { get; set; }

        public string StreamId { get; set; }

        public ProtocolSettings ProtocolSettings { get; set; }
    }
}
