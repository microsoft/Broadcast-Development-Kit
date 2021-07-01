// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Linq;
using Application.Exceptions;
using BotService.Infrastructure.Core;
using BotService.Infrastructure.Extensions;
using Domain.Enums;
using Gst;
using Gst.App;

namespace BotService.Infrastructure.Pipelines
{
    public class SrtCpuEncodingMediaPipeline : IMediaExtractionPipeline
    {
        private const int EncoderTune = 0x00000004;
        private const int EncoderBitrate = 2500;
        private const int EncoderSpeedPreset = 2;
        private const int KeyFrameDistance = 60;
        private const string VideoColorimetry = "bt709";
        private const int VideoWidth = 1920;
        private const int VideoHeight = 1080;
        private const string VideoFrameRate = "30000/1001";

        private readonly object _videoSrcLock = new object();
        private readonly object _audioSrcLock = new object();
        private readonly Pipeline _pipeline;
        private readonly SrtSettings _protocolSettings;

        private AppSrc _videoSrc, _audioSrc;
        private Element _muxer, _sinkQueue, _sink;
        private Element _audioQueue, _audioConvert, _audioConvertFilter, _audioResample, _audioResampleFilter, _audioEncoder, _audioParse;
        private Element _videoOverlay, _videoParse;
        private Element _videoQueue, _videoDecoder, _videoConvert, _colorimetryFilter, _videoScale, _videoScaleFilter, _videoRate, _videoRateFilter, _videoEncoder;

        // Test Media Platform timestamps
        private ulong _baseTimestamp;
        private Element _audioIdentity, _videoIdentity;

        public SrtCpuEncodingMediaPipeline(SrtSettings protocolSettings)
        {
            _pipeline = new Pipeline();
            _protocolSettings = protocolSettings;

            if (!BuildPipeline())
            {
                throw new StartStreamExtractionException("Media pipeline could not be created");
            }

            Bus = _pipeline.Bus;
        }

        public Bus Bus { get; set; }

        public StateChangeReturn Play()
        {
            _baseTimestamp = (ulong)((System.DateTime.UtcNow - new System.DateTime(1900, 1, 1)).Ticks * 100);
            return _pipeline.SetState(State.Playing);
        }

        public StateChangeReturn Stop()
        {
            return _pipeline.SetState(State.Null);
        }

        public void PushAudioBuffer(byte[] buffer, long timestamp)
        {
            var gstBuffer = new Gst.Buffer(null, (ulong)buffer.Length, Gst.AllocationParams.Zero);
            gstBuffer.Fill(0, buffer);
            var referencedTimestamp = ((ulong)(timestamp * 100)) - _baseTimestamp;
            gstBuffer.Pts = referencedTimestamp;
            gstBuffer.Dts = referencedTimestamp;

            lock (_audioSrcLock)
            {
                _audioSrc.PushBuffer(gstBuffer);
            }

            gstBuffer.Dispose();
        }

        public void PushVideoBuffer(byte[] buffer, long timestamp, int width, int height)
        {
            var gstBuffer = new Gst.Buffer(null, (ulong)buffer.Length, Gst.AllocationParams.Zero);
            gstBuffer.Fill(0, buffer);
            var referencedTimestamp = ((ulong)(timestamp * 100)) - _baseTimestamp;
            gstBuffer.Pts = referencedTimestamp;
            gstBuffer.Dts = referencedTimestamp;
            gstBuffer.Duration = referencedTimestamp;

            lock (_videoSrcLock)
            {
                _videoSrc.PushBuffer(gstBuffer);
            }

            gstBuffer.Dispose();
        }

        private bool BuildPipeline()
        {
            CreatePipelineElements();
            AddPipelineElements();

            var linkedElements = LinkPipelineElements();

            return linkedElements;
        }

        private void CreatePipelineElements()
        {
            // Streaming elements
            _muxer = ElementFactory.Make("mpegtsmux", "muxer");
            _sinkQueue = ElementFactory.Make("queue", "sink_queue");
            _sink = ElementFactory.Make("srtsink", "srt_output");

            var uri = _protocolSettings.Mode == SrtMode.Caller ? _protocolSettings.Url : $"srt://:{_protocolSettings.Port}";

            _sink.SetProperty("uri", new GLib.Value(uri));
            _sink.SetProperty("mode", new GLib.Value((int)_protocolSettings.Mode));
            _sink.SetProperty("latency", new GLib.Value(_protocolSettings.Latency));
            _sink.SetProperty("passphrase", new GLib.Value(_protocolSettings.Passphrase));
            _sink.SetProperty("wait-for-connection", new GLib.Value(false));

            // Video processing elements
            _videoSrc = new AppSrc("video_src")
            {
                Caps = Caps.FromString("video/x-h264, stream-format=byte-stream, alignment=nal, profile=constrained-high, level=4"),
                Format = Format.Time,
                IsLive = true,
                DoTimestamp = false,
            };

            _videoQueue = ElementFactory.Make("queue", "video_src_queue");
            _videoDecoder = ElementFactory.Make("avdec_h264", "video_decoder");
            _videoConvert = ElementFactory.Make("videoconvert", "video_convert");
            _colorimetryFilter = ElementFactory.Make("capsfilter", "colorimetry_filter");
            _colorimetryFilter.SetProperty("caps", new GLib.Value(Caps.FromString($"video/x-raw, colorimetry={VideoColorimetry}")));
            _videoScale = ElementFactory.Make("videoscale", "video_scale");
            _videoScaleFilter = ElementFactory.Make("capsfilter", "video_scale_filter");
            _videoScaleFilter.SetProperty("caps", new GLib.Value(Caps.FromString($"video/x-raw, width={VideoWidth}, height={VideoHeight}, pixel-aspect-ratio=1/1")));
            _videoRate = ElementFactory.Make("videorate", "video_rate");
            _videoRateFilter = ElementFactory.Make("capsfilter", "video_rate_filter");
            _videoRateFilter.SetProperty("caps", new GLib.Value(Caps.FromString($"video/x-raw, framerate={VideoFrameRate}")));
            _videoOverlay = _protocolSettings.TimeOverlay ? ElementFactory.Make("timeoverlay", "time_overlay") : null;
            _videoEncoder = ElementFactory.Make("x264enc", "video_encoder");
            _videoEncoder.SetProperty("tune", new GLib.Value(EncoderTune));
            _videoEncoder.SetProperty("bitrate", new GLib.Value(EncoderBitrate));
            _videoEncoder.SetProperty("speed-preset", new GLib.Value(EncoderSpeedPreset));
            _videoEncoder.SetProperty("key-int-max", new GLib.Value(KeyFrameDistance));
            _videoParse = ElementFactory.Make("h264parse", "video_parse");

            // Audio stream processing plugins
            _audioSrc = new AppSrc("audio_src")
            {
                Caps = Caps.FromString("audio/x-raw, format=S16LE, layout=interleaved, rate=16000, channels=1"),
                Format = Format.Time,
                IsLive = true,
                DoTimestamp = false,
            };

            _audioQueue = ElementFactory.Make("queue", "audio_src_queue");
            _audioConvert = ElementFactory.Make("audioconvert", "audio_convert");
            _audioConvertFilter = ElementFactory.Make("capsfilter");
            _audioConvertFilter.SetProperty("caps", new GLib.Value(Caps.FromString("audio/x-raw, channels=2")));
            _audioResample = ElementFactory.Make("audioresample", "audio_resample");
            _audioResampleFilter = ElementFactory.Make("capsfilter");
            _audioResampleFilter.SetProperty("caps", new GLib.Value(Caps.FromString($"audio/x-raw, rate={_protocolSettings.AudioFormat.ToAudioRate()}")));
            _audioEncoder = ElementFactory.Make("avenc_aac", "audio_encoder");
            _audioParse = ElementFactory.Make("aacparse", "audio_parse");

            // Timestamps test
            _videoIdentity = ElementFactory.Make("identity", "video_identity");
            _videoIdentity.SetProperty("silent", new GLib.Value(false));
            _videoIdentity.SetProperty("check-imperfect-timestamp", new GLib.Value(true));
            _audioIdentity = ElementFactory.Make("identity", "audio_identity");
            _audioIdentity.SetProperty("silent", new GLib.Value(false));
            _audioIdentity.SetProperty("check-imperfect-timestamp", new GLib.Value(true));
        }

        private void AddPipelineElements()
        {
            var videoElements = new[] { _videoSrc, _videoQueue, _videoDecoder, _videoConvert, _colorimetryFilter, _videoScale, _videoScaleFilter, _videoRate, _videoRateFilter, _videoOverlay, _videoEncoder, _videoParse, _videoIdentity };

            _pipeline.Add(_muxer, _sinkQueue, _sink);
            _pipeline.Add(_audioSrc, _audioQueue, _audioConvert, _audioConvertFilter, _audioResample, _audioResampleFilter, _audioEncoder, _audioParse, _audioIdentity);
            _pipeline.Add(videoElements.Where(e => e != null).ToArray());
        }

        private bool LinkPipelineElements()
        {
            var videoElements = new[] { _videoSrc, _videoQueue, _videoParse, _videoDecoder, _videoConvert, _colorimetryFilter, _videoScale, _videoScaleFilter, _videoRate, _videoRateFilter, _videoOverlay, _videoEncoder, _videoIdentity, _muxer };

            return Element.Link(videoElements.Where(e => e != null).ToArray()) &&
                   Element.Link(_audioSrc, _audioQueue, _audioConvert, _audioConvertFilter, _audioResample, _audioResampleFilter, _audioEncoder, _audioParse, _audioIdentity, _muxer) &&
                   Element.Link(_muxer, _sinkQueue, _sink);
        }
    }
}
