// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Microsoft.Skype.Bots.Media;

namespace BotService.Application.Core
{
    public interface IMediaInjector
    {
        IVideoSocket VideoSocket { get; }

        void Start(MediaInjectionSettings injectionSettings);

        void Stop();
    }
}
