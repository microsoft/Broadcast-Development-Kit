// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Domain.Enums;
using Gst.Audio;

namespace BotService.Infrastructure.Extensions
{
    public static class StreamVolumeExtensions
    {
        public static Gst.Audio.StreamVolumeFormat ToStreamVolumeFormat(this Domain.Enums.StreamVolumeFormat streamVolumeType)
        {
            var streamVoumeFormat = (Gst.Audio.StreamVolumeFormat)Enum.ToObject(typeof(Gst.Audio.StreamVolumeFormat), streamVolumeType);

            return streamVoumeFormat;
        }
    }
}
