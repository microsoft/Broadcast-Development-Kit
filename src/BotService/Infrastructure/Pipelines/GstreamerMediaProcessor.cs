// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using BotService.Application.Core;
using BotService.Infrastructure.Core;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Skype.Bots.Media;

namespace BotService.Infrastructure.Pipelines
{
    public class GstreamerMediaProcessor : IMediaProcessor
    {
        private readonly ILogger _logger;
        private readonly ProtocolSettings _protocolSettings;
        private IMediaExtractionPipeline _pipeline;

        public GstreamerMediaProcessor(ProtocolSettings protocolSettings, ILoggerFactory loggerFactory)
        {
            _protocolSettings = protocolSettings ?? throw new ArgumentNullException(nameof(protocolSettings));
            _logger = loggerFactory.CreateLogger<GstreamerMediaProcessor>();
        }

        public void Play()
        {
            _pipeline = CreatePipeline(_protocolSettings);
            _pipeline.Bus.EnableSyncMessageEmission();
            _pipeline.Bus.SyncMessage += OnBusMessage;

            _pipeline.Play();
        }

        public void Stop()
        {
            if (_pipeline != null)
            {
                _pipeline.Stop();
            }
        }

        public void PushAudioBuffer(byte[] buffer, AudioFormat audioFormat, long timestamp, int rate)
        {
            _pipeline.PushAudioBuffer(buffer, timestamp);
        }

        public void PushVideoBuffer(byte[] buffer, VideoColorFormat videoFormat, long timestamp, int width, int height)
        {
            _pipeline.PushVideoBuffer(buffer, timestamp, width, height);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _pipeline != null)
            {
                _pipeline.Stop();
            }
        }

        private static IMediaExtractionPipeline CreatePipeline(ProtocolSettings protocol)
        {
            IMediaExtractionPipeline mediaPipeline;

            switch (protocol.Type)
            {
                case Protocol.SRT:
                    var srtProtocolSettings = protocol as SrtSettings;
                    mediaPipeline = new SrtCpuEncodingMediaPipeline(srtProtocolSettings);

                    return mediaPipeline;
                case Protocol.RTMP:
                    var rtmpProtocolSettings = protocol as RtmpSettings;
                    mediaPipeline = new RtmpCpuEncodingMediaPipeline(rtmpProtocolSettings);

                    return mediaPipeline;
                default:
                    throw new ArgumentException("Protocol not supported", nameof(protocol));
            }
        }

        private void OnBusMessage(object sender, GLib.SignalArgs args)
        {
            var msg = (Gst.Message)args.Args[0];

            switch (msg.Type)
            {
                case Gst.MessageType.Error:
                    var structure = msg.ParseErrorDetails();
                    msg.ParseError(out GLib.GException err, out string debug);
                    _logger.LogError("[MediaPipeline] Error received from element {Name}: {Message}", msg.Src.Name, err.Message);
                    _logger.LogError("[MediaPipeline] Debugging information: {0}", debug ?? "none");
                    _logger.LogError("[MediaPipeline] {structure}", structure);
                    break;
                case Gst.MessageType.StateChanged:
                    var element = (Gst.Element)msg.Src;
                    _logger.LogInformation("[MediaPipeline] Pipeline state changed message from: {Name}, current state: {CurrentState}", msg.Src.Name, element.CurrentState);
                    break;
                case Gst.MessageType.StreamStatus:
                    _logger.LogInformation("[MediaPipeline] Pipeline stream status message from: {Name}", msg.Src.Parent.Name);
                    break;
                case Gst.MessageType.Qos:
                    msg.ParseQosStats(out Gst.Format format, out ulong processed, out ulong dropped);
                    _logger.LogInformation("[MediaPipeline] QoS message: format = {format}, processed = {processed}, dropped = {dropped}", format, processed, dropped);
                    break;
                case Gst.MessageType.Warning:
                    msg.ParseWarning(out _, out string debugMessage);
                    _logger.LogWarning("[MediaPipeline] Warning message from element: {Name}, debug message: {debugMessage}", msg.Src.Name, debugMessage);
                    break;
                default:
                    _logger.LogInformation("[MediaPipeline] default message type: {Type}, from element: {Name}", msg.Type, msg.Src.Name);
                    break;
            }
        }
    }
}
