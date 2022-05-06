// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace BotService.Infrastructure.Core
{
    public class StreamVolume
    {
        public StreamVolumeFormat Format { get; set; }

        public double Value { get; set; }
    }
}
