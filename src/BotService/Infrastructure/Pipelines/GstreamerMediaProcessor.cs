using BotService.Application.Core;
using BotService.Infrastructure.Core;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Skype.Bots.Media;
using System;

namespace BotService.Infrastructure.Pipelines
{
    public class GstreamerMediaProcessor : IMediaProcessor
    { 
        private IMediaExtractionPipeline pipeline;
        private readonly ILogger logger;
        private readonly ProtocolSettings protocolSettings;

        public GstreamerMediaProcessor(ProtocolSettings protocolSettings, ILoggerFactory loggerFactory)
        {
            this.protocolSettings = protocolSettings ?? throw new ArgumentNullException(nameof(protocolSettings));
            this.logger = loggerFactory.CreateLogger<GstreamerMediaProcessor>();
        }

        public void Play()
        {
            this.pipeline = CreatePipeline(protocolSettings);
            this.pipeline.Bus.EnableSyncMessageEmission();
            this.pipeline.Bus.SyncMessage += OnBusMessage;

            this.pipeline.Play();
        }

        public void Stop()
        {
            if (this.pipeline != null)
            {
                this.pipeline.Stop();
            }
        }

        public void PushAudioBuffer(byte[] buffer, AudioFormat audioFormat, long timestamp, int rate)
        {
            this.pipeline.PushAudioBuffer(buffer, timestamp);
        }

        public void PushVideoBuffer(byte[] buffer, VideoColorFormat videoFormat, long timestamp, int width, int height)
        {
            this.pipeline.PushVideoBuffer(buffer, timestamp, width, height);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.pipeline != null)
            {
                this.pipeline.Stop();
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
                    this.logger.LogError("[MediaPipeline] Error received from element {Name}: {Message}", msg.Src.Name, err.Message);
                    this.logger.LogError("[MediaPipeline] Debugging information: {0}", debug ?? "none");
                    this.logger.LogError("[MediaPipeline] {structure}", structure);
                    break;
                case Gst.MessageType.StateChanged:
                    var element = (Gst.Element)msg.Src;
                    this.logger.LogInformation("[MediaPipeline] Pipeline state changed message from: {Name}, current state: {CurrentState}", msg.Src.Name, element.CurrentState);
                    break;
                case Gst.MessageType.StreamStatus:
                    this.logger.LogInformation("[MediaPipeline] Pipeline stream status message from: {Name}", msg.Src.Parent.Name);
                    break;
                case Gst.MessageType.Qos:
                    msg.ParseQosStats(out Gst.Format format, out ulong processed, out ulong dropped);
                    this.logger.LogInformation("[MediaPipeline] QoS message: format = {format}, processed = {processed}, dropped = {dropped}", format, processed, dropped);
                    break;
                case Gst.MessageType.Warning:
                    msg.ParseWarning(out _, out string debugMessage);
                    this.logger.LogWarning("[MediaPipeline] Warning message from element: {Name}, debug message: {debugMessage}", msg.Src.Name, debugMessage);
                    break;
                default:
                    this.logger.LogInformation("[MediaPipeline] default message type: {Type}, from element: {Name}", msg.Type, msg.Src.Name);
                    break;
            }
        }
    }
}
