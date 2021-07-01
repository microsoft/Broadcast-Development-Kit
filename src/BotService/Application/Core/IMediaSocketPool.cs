// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Microsoft.Skype.Bots.Media;

namespace BotService.Application.Core
{
    public interface IMediaSocketPool
    {
        IAudioSocket MainAudioSocket { get; }

        IVideoSocket GetScreenShareSocket();

        IVideoSocket GetParticipantVideoSocket();

        IVideoSocket GetInjectionVideoSocket();

        void ReleaseSocket(IVideoSocket socket);
    }
}
