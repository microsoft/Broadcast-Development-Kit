// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Microsoft.Skype.Bots.Media;

namespace BotService.Application.Core
{
    public interface IMediaSocketPool
    {
        IAudioSocket MainAudioSocket { get; }

        IVideoSocket InjectionSocket { get; }

        IVideoSocket GetScreenShareSocket();

        IVideoSocket GetParticipantVideoSocket();

        void ReleaseSocket(IVideoSocket socket);
    }
}
