// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using BotService.Application.Core;
using BotService.Infrastructure.Common.Logging;
using BotService.Infrastructure.Pipelines;
using Gst;
using Gst.App;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Communications.Calls.Media;
using Microsoft.Skype.Bots.Media;

namespace BotService.Infrastructure.Core
{
    public class SlateMediaInjector : IMediaInjector, IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<SlateMediaInjector> _logger;
        private readonly PipelineBusObserver _pipelineBusObserver;

        private IDisposable _unsubscribeObserver;
        private IMediaInjectionPipeline _pipeline;

        public SlateMediaInjector(IVideoSocket videoSocket, ILoggerFactory loggerFactory, PipelineBusObserver pipelineBusObserver)
        {
            VideoSocket = videoSocket;
            _loggerFactory = loggerFactory;
            _pipelineBusObserver = pipelineBusObserver;
            _logger = _loggerFactory.CreateLogger<SlateMediaInjector>();
        }

        public IVideoSocket VideoSocket { get; protected set; }

        public void Start(MediaInjectionSettings injectionSettings)
        {
            _pipeline = new SlateMediaInjectionPipeline(injectionSettings, _loggerFactory);
            _pipeline.SetNewVideoSampleHandler(NewVideoSample);
            _unsubscribeObserver = _pipeline.Subscribe(_pipelineBusObserver);
            _pipeline.Play();
        }

        public void Stop()
        {
            _pipeline.Stop();
            _pipeline.RemoveNewVideoSampleHandler(NewVideoSample);
            _unsubscribeObserver?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pipeline = null;
            }
        }

        private void NewVideoSample(object sender, GLib.SignalArgs args)
        {
            var sink = (AppSink)sender;
            var sample = sink.PullSample();

            if (sample != null)
            {
                var buffer = sample.Buffer;
                buffer.Map(out MapInfo info, MapFlags.Read);
                var size = (uint)info.Size;
                var data = info.Data;
                var timestamp = MediaPlatform.GetCurrentTimestamp();
                var videoSendBuffer = new VideoSendBuffer(data, size, VideoFormat.NV12_1920x1080_1_875Fps, timestamp);

                try
                {
                    VideoSocket.Send(videoSendBuffer);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Slate Injection] Video - Error sending buffer - Message: {message}", ex.InnerException?.Message ?? ex.Message);
                }

                buffer.Unmap(info);
                buffer.Dispose();
                sample.Dispose();
            }
            else
            {
                _logger.LogWarning("[Slate Injection] Video - New sample signal was triggered without a sample.");
            }
        }
    }
}
