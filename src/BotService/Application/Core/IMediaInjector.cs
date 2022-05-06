// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using BotService.Infrastructure.Core;
using Microsoft.Skype.Bots.Media;

namespace BotService.Application.Core
{
    public interface IMediaInjector
    {
        IVideoSocket VideoSocket { get; }

        bool SourceConnected { get; }

        void Start(MediaInjectionSettings injectionSettings);

        void Stop();

        void SetVolume(StreamVolume streamVolume);

        void SwitchContentStatus(bool shouldInject);

        void SetOnStreamStateChanged(Action onStreamStateChanged);
    }
}
