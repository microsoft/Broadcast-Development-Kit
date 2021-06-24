using Application.Exceptions;
using BotService.Infrastructure.Core;
using Domain.Enums;
using Gst;
using Gst.App;
using System;
using System.Linq;

namespace BotService.Infrastructure.Pipelines
{
    public class RtmpCpuEncodingMediaPipeline : IMediaExtractionPipeline
    {
        public Bus Bus { get; set; }
        private const int QUEUE_LEAKY_TYPE = 2;
        private const int ENCODER_TUNE = 0x00000004;
        private const int ENCODER_BITRATE = 2500;
        private const int ENCODER_SPEED_PRESET = 2;
        private const int KEY_FRAME_DISTANCE = 60;
        private const string VIDEO_COLORIMETRY = "bt709";
        private const int VIDEO_WIDTH = 1920;
        private const int VIDEO_HEIGHT = 1080;
        private const string VIDEO_FRAME_RATE = "30000/1001";
        private readonly Pipeline pipeline;
        private readonly RtmpSettings protocolSettings;
        private AppSrc videoSrc, audioSrc;
        private Element muxer, sinkQueue, sink;
        private Element audioQueue, audioConvert, audioConvertFilter, audioResample, audioResampleFilter, audioEncoder, audioParse;
        private Element videoOverlay, videoParse;
        private Element videoQueue, videoDecoder, videoConvert, colorimetryFilter, videoScale, videoScaleFilter, videoRate, videoRateFilter, videoEncoder;

        private readonly object videoSrcLock = new object();
        private readonly object audioSrcLock = new object();

        //Test Media Platform timestamps
        private ulong baseTimestamp;
        private Element audioIdentity, videoIdentity;

        public RtmpCpuEncodingMediaPipeline(RtmpSettings protocolSettings)
        {
            this.pipeline = new Pipeline();
            this.protocolSettings = protocolSettings;

            if (!this.BuildPipeline())
            {
                throw new StartStreamExtractionException("Media pipeline could not be created");
            }

            this.Bus = this.pipeline.Bus;
        }

        public StateChangeReturn Play()
        {
            this.baseTimestamp = (ulong)((System.DateTime.UtcNow - new System.DateTime(1900, 1, 1)).Ticks * 100);

            return this.pipeline.SetState(State.Playing);
        }

        public StateChangeReturn Stop()
        {
            return this.pipeline.SetState(State.Null);
        }

        public void PushAudioBuffer(byte[] buffer, long timestamp)
        {
            var gstBuffer = new Gst.Buffer(null, (ulong)buffer.Length, Gst.AllocationParams.Zero);
            gstBuffer.Fill(0, buffer);
            var referencedTimestamp = ((ulong)(timestamp * 100)) - this.baseTimestamp;

            gstBuffer.Pts = referencedTimestamp;
            gstBuffer.Dts = referencedTimestamp;

            lock (audioSrcLock)
            {
                audioSrc.PushBuffer(gstBuffer);
            }

            gstBuffer.Dispose();
        }

        public void PushVideoBuffer(byte[] buffer, long timestamp, int width, int height)
        {
            var gstBuffer = new Gst.Buffer(null, (ulong)buffer.Length, Gst.AllocationParams.Zero);
            gstBuffer.Fill(0, buffer);
            var referencedTimestamp = ((ulong)(timestamp * 100)) - this.baseTimestamp;

            gstBuffer.Pts = referencedTimestamp;
            gstBuffer.Dts = referencedTimestamp;

            lock (videoSrcLock)
            {
                videoSrc.PushBuffer(gstBuffer);
            }

            gstBuffer.Dispose();
        }

        private bool BuildPipeline()
        {
            this.CreatePipelineElements();
            this.AddPipelineElements();

            var linkedElements = LinkPipelineElements();

            return linkedElements;
        }

        private void CreatePipelineElements()
        {
            // Streaming elements
            this.muxer = ElementFactory.Make("flvmux", "muxer");
            this.muxer.SetProperty("streamable", new GLib.Value(true));
            this.muxer.SetProperty("latency", new GLib.Value(500000000));
            this.sinkQueue = ElementFactory.Make("queue", "sink_queue");

            this.sinkQueue.SetProperty("leaky", new GLib.Value(QUEUE_LEAKY_TYPE));
            this.sink = ElementFactory.Make("rtmpsink", "rtmp_output");
            var uri = String.IsNullOrEmpty(protocolSettings.StreamKey) ? protocolSettings.StreamUrl : $"{protocolSettings.StreamUrl}/{protocolSettings.StreamKey}";
            this.sink.SetProperty("location", new GLib.Value(uri));

            // Video processing elements 
            this.videoSrc = new AppSrc("video_src")
            {
                Caps = Caps.FromString("video/x-h264, stream-format=byte-stream, alignment=nal, profile=constrained-high, level=4"),
                Format = Format.Time,
                IsLive = true,
                DoTimestamp = false
            };

            this.videoQueue = ElementFactory.Make("queue", "video_src_queue");
            this.videoQueue.SetProperty("leaky", new GLib.Value(QUEUE_LEAKY_TYPE));
            this.videoDecoder = ElementFactory.Make("avdec_h264", "video_decoder");
            this.videoConvert = ElementFactory.Make("videoconvert", "video_convert");
            this.colorimetryFilter = ElementFactory.Make("capsfilter", "colorimetry_filter");
            this.colorimetryFilter.SetProperty("caps", new GLib.Value(Caps.FromString($"video/x-raw, colorimetry={VIDEO_COLORIMETRY}")));
            this.videoScale = ElementFactory.Make("videoscale", "video_scale");
            this.videoScaleFilter = ElementFactory.Make("capsfilter", "video_scale_filter");
            this.videoScaleFilter.SetProperty("caps", new GLib.Value(Caps.FromString($"video/x-raw, width={VIDEO_WIDTH}, height={VIDEO_HEIGHT}, pixel-aspect-ratio=1/1")));
            this.videoRate = ElementFactory.Make("videorate", "video_rate");
            this.videoRateFilter = ElementFactory.Make("capsfilter", "video_rate_filter");
            this.videoRateFilter.SetProperty("caps", new GLib.Value(Caps.FromString($"video/x-raw, framerate={VIDEO_FRAME_RATE}")));
            this.videoOverlay = this.protocolSettings.TimeOverlay ? ElementFactory.Make("timeoverlay", "time_overlay") : null;
            this.videoEncoder = ElementFactory.Make("x264enc", "video_encoder");
            this.videoEncoder.SetProperty("tune", new GLib.Value(ENCODER_TUNE));
            this.videoEncoder.SetProperty("bitrate", new GLib.Value(ENCODER_BITRATE));
            this.videoEncoder.SetProperty("speed-preset", new GLib.Value(ENCODER_SPEED_PRESET));
            this.videoEncoder.SetProperty("key-int-max", new GLib.Value(KEY_FRAME_DISTANCE));
            this.videoParse = ElementFactory.Make("h264parse", "video_parse");

            // Audio stream processing plugins
            this.audioSrc = new AppSrc("audio_src")
            {
                Caps = Caps.FromString("audio/x-raw, format=S16LE, layout=interleaved, rate=16000, channels=1"),
                Format = Format.Time,
                IsLive = true,
                DoTimestamp = false
            };

            this.audioQueue = ElementFactory.Make("queue", "audio_src_queue");

            this.audioQueue.SetProperty("leaky", new GLib.Value(QUEUE_LEAKY_TYPE));
            this.audioConvert = ElementFactory.Make("audioconvert", "audio_convert");
            this.audioConvertFilter = ElementFactory.Make("capsfilter");
            this.audioConvertFilter.SetProperty("caps", new GLib.Value(Caps.FromString("audio/x-raw, channels=2")));
            this.audioResample = ElementFactory.Make("audioresample", "audio_resample");
            this.audioResampleFilter = ElementFactory.Make("capsfilter");
            this.audioResampleFilter.SetProperty("caps", new GLib.Value(Caps.FromString($"audio/x-raw, rate={this.protocolSettings.AudioFormat.ToAudioRate()}")));
            this.audioEncoder = ElementFactory.Make("avenc_aac", "audio_encoder");
            this.audioParse = ElementFactory.Make("aacparse", "audio_parse");

            // Timestamp test
            this.videoIdentity = ElementFactory.Make("identity", "video_identity");
            this.videoIdentity.SetProperty("silent", new GLib.Value(false));
            this.videoIdentity.SetProperty("check-imperfect-timestamp", new GLib.Value(true));
            this.audioIdentity = ElementFactory.Make("identity", "audio_identity");
            this.audioIdentity.SetProperty("silent", new GLib.Value(false));
            this.audioIdentity.SetProperty("check-imperfect-timestamp", new GLib.Value(true));

        }

        private void AddPipelineElements()
        {
            var videoElements = new[] { this.videoSrc, this.videoQueue, this.videoDecoder, this.videoConvert, this.colorimetryFilter, this.videoScale, this.videoScaleFilter, this.videoRate, this.videoRateFilter, this.videoOverlay, this.videoEncoder, this.videoParse, this.videoIdentity };

            this.pipeline.Add(this.muxer, this.sinkQueue, this.sink);
            this.pipeline.Add(this.audioSrc, this.audioQueue, this.audioConvert, this.audioConvertFilter, this.audioResample, this.audioResampleFilter, this.audioEncoder, this.audioParse, this.audioIdentity);
            this.pipeline.Add(videoElements.Where(e => e != null).ToArray());
        }

        private bool LinkPipelineElements()
        {
            var videoElements = new[] { this.videoSrc, this.videoQueue, this.videoParse, this.videoDecoder, this.videoConvert, this.colorimetryFilter, this.videoScale, this.videoScaleFilter, this.videoRate, this.videoRateFilter, this.videoOverlay, this.videoEncoder, this.videoIdentity, this.muxer };

            // TODO: refactor this if statement
            return Element.Link(this.audioSrc, this.audioQueue, this.audioConvert, this.audioConvertFilter, this.audioResample, this.audioResampleFilter, this.audioEncoder, this.audioParse, this.audioIdentity, this.muxer) &&
                   Element.Link(videoElements.Where(e => e != null).ToArray()) &&
                   Element.Link(this.muxer, this.sinkQueue, this.sink);
        }
    }
}

