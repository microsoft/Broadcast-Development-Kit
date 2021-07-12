// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using BotService.Infrastructure.Core;
using Microsoft.Skype.Bots.Media;

namespace BotService.Infrastructure.Pipelines
{
    public interface IMediaProcessor : IDisposable
    {
        void Play();

        void Stop();

        void PushAudioBuffer(byte[] buffer, AudioFormat audioFormat, long timestamp, int rate);

        void PushVideoBuffer(byte[] buffer, VideoColorFormat videoFormat, long timestamp, int width, int height);
    }
}
