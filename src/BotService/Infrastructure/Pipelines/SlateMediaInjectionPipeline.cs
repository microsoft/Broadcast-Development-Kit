// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.IO;
using Application.Exceptions;
using BotService.Application.Core;
using BotService.Infrastructure.Common.Logging;
using Domain.Enums;
using Gst;
using Gst.App;
using Microsoft.Extensions.Logging;

namespace BotService.Infrastructure.Pipelines
{
    public class SlateMediaInjectionPipeline : IMediaInjectionPipeline
    {
        private const string VideoFrameRate = "15/8";
        private const string SlateImageFileName = "Slate.png";
        private readonly MediaInjectionSettings _injectionSettings;
        private readonly ILogger _logger;
        private readonly Bus _bus;
        private readonly AppSink _appSink;
        private readonly Pipeline _pipeline;

        private PipelineBusObserver _pipelineBusObserver;

        public SlateMediaInjectionPipeline(
            MediaInjectionSettings injectionSettings,
            ILoggerFactory loggerFactory)
        {
            _injectionSettings = injectionSettings;
            _logger = loggerFactory.CreateLogger<MediaInjectionPipeline>();

            var name = $"pipeline_slate_{injectionSettings.StreamId}";

            _pipeline = new Pipeline(name);
            _appSink = new AppSink("Slate AppSink")
            {
                EmitSignals = true,
            };

            _bus = _pipeline.Bus;

            var pngSrc = ElementFactory.Make("filesrc");
            var pngDecoder = ElementFactory.Make("pngdec");
            var imageFreeze = ElementFactory.Make("imagefreeze");
            var videoConvert = ElementFactory.Make("videoconvert");
            var capsFilter = ElementFactory.Make("capsfilter");
            var caps = Caps.FromString($"video/x-raw, format=NV12, framerate={VideoFrameRate}");
            var rootPath = Directory.GetCurrentDirectory();

            pngSrc.SetProperty("location", new GLib.Value($"{rootPath}/{SlateImageFileName}"));
            capsFilter.SetProperty("caps", new GLib.Value(caps));

            _pipeline.Add(pngSrc, pngDecoder, imageFreeze, videoConvert, capsFilter, _appSink);

            var linked = Element.Link(pngSrc, pngDecoder, imageFreeze, videoConvert, capsFilter, _appSink);

            if (!linked)
            {
                _logger.LogError("Could not link video processing elements for slate stream.");

                throw new StartStreamInjectionException("Error linking video processing elements");
            }
        }

        public (State State, State NextState) GetState()
        {
            _pipeline.GetState(out State state, out State pending, 1000);

            return (state, pending);
        }

        public void Play()
        {
            _bus.EnableSyncMessageEmission();
            _bus.SyncMessage += OnBusMessage;
            _pipeline.SetState(State.Playing);
            _logger.LogInformation("[Slate Media Injection] Started injection");
        }

        public void Stop()
        {
            _pipeline.SetState(State.Null);
            _bus.SyncMessage -= OnBusMessage;
            _bus.Unref();
            _pipeline.Unref();

            _logger.LogInformation("[Slate Media Injection] Stopped injection");
        }

        public IDisposable Subscribe(IObserver<BusEventPayload> observer)
        {
            _pipelineBusObserver = observer as PipelineBusObserver;

            return new PipelineBusObserverUnsuscriber(_pipelineBusObserver);
        }

        public void SetNewVideoSampleHandler(NewSampleHandler newVideoSampleHandler)
        {
            _appSink.NewSample += newVideoSampleHandler;
        }

        public void RemoveNewVideoSampleHandler(NewSampleHandler newVideoSampleHandler)
        {
            _appSink.NewSample -= newVideoSampleHandler;
        }

        public void RemoveNewAudioSampleHandler(NewSampleHandler newAudioSampleHandler)
        {
            throw new NotSupportedException();
        }

        public void SetNewAudioSampleHandler(NewSampleHandler newAudioSampleHandler)
        {
            throw new NotSupportedException();
        }

        private static (BusMessageType messageType, string message) GetFormatedMessage(Message msg)
        {
            var messageType = BusMessageType.Unknown;
            var formatedMessage = string.Empty;

            switch (msg.Type)
            {
                case MessageType.Error:
                    var structure = msg.ParseErrorDetails();
                    msg.ParseError(out GLib.GException err, out string debug);
                    messageType = BusMessageType.Error;
                    formatedMessage = $"Error received from element {msg.Src.Name}: {err.Message}, Debugging information {debug ?? "none"} {structure}";
                    break;
                case MessageType.StateChanged:
                    var element = (Element)msg.Src;

                    // Format only the state change of the pipeline, not elements
                    if (element.Name.Contains("pipeline"))
                    {
                        messageType = BusMessageType.StateChanged;
                        formatedMessage = $"State changed message from: {msg.Src.Name}, Current state: {element.CurrentState}";
                    }

                    break;
            }

            return (messageType, formatedMessage);
        }

        private void OnBusMessage(object sender, GLib.SignalArgs args)
        {
            var msg = (Message)args.Args[0];
            (BusMessageType messageType, string formatedMessage) = GetFormatedMessage(msg);

            if (!string.IsNullOrEmpty(formatedMessage))
            {
                var busEvent = new BusEventPayload
                {
                    CallId = _injectionSettings.CallId,
                    StreamId = _injectionSettings.StreamId,
                    MessageType = messageType,
                    Message = formatedMessage,
                    DateTime = System.DateTime.Now,
                };

                _pipelineBusObserver.OnNext(busEvent);
            }
        }
    }
}
