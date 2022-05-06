// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using BotService.Infrastructure.Common.Logging;
using BotService.Infrastructure.Core;
using Gst;
using Gst.App;

namespace BotService.Infrastructure.Pipelines
{
    public interface IMediaInjectionPipeline
    {
        (State State, State NextState) GetState();

        void Play();

        void RemoveBufferReceivedHandler(Action onBufferReceived);

        void RemoveNewAudioSampleHandler(NewSampleHandler newAudioSampleHandler);

        void RemoveNewVideoSampleHandler(NewSampleHandler newVideoSampleHandler);

        void SetBufferReceivedHandler(Action onBufferReceived);

        void SetNewAudioSampleHandler(NewSampleHandler newAudioSampleHandler);

        void SetNewVideoSampleHandler(NewSampleHandler newVideoSampleHandler);

        void Stop();

        void SetVolume(StreamVolume streamVolume);

        IDisposable Subscribe(IObserver<BusEventPayload> observer);
    }
}
