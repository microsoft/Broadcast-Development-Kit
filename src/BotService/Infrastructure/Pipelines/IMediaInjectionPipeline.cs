using System;
using BotService.Infrastructure.Common.Logging;
using Gst;
using Gst.App;

namespace BotService.Infrastructure.Pipelines
{
    public interface IMediaInjectionPipeline
    {
        (State State, State NextState) GetState();

        void Play();

        void RemoveNewAudioSampleHandler(NewSampleHandler newAudioSampleHandler);

        void RemoveNewVideoSampleHandler(NewSampleHandler newVideoSampleHandler);

        void SetNewAudioSampleHandler(NewSampleHandler newAudioSampleHandler);

        void SetNewVideoSampleHandler(NewSampleHandler newVideoSampleHandler);

        void Stop();

        IDisposable Subscribe(IObserver<BusEventPayload> observer);
    }
}
