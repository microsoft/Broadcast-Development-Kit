// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Domain.Enums;

namespace BotService.Infrastructure.Extensions
{
    public static class SupportedAudioExtensions
    {
        public static int ToAudioRate(this SupportedAudioFormat supportedAudio)
        {
            switch (supportedAudio)
            {
                case SupportedAudioFormat.AAC_44100:
                    return 44100;
                case SupportedAudioFormat.AAC_48000:
                    return 48000;
                default:
                    throw new ArgumentOutOfRangeException(nameof(supportedAudio), "Unsupported audio");
            }
        }
    }
}
