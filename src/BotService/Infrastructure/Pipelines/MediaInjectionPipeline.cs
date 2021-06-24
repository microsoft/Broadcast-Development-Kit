using Application.Exceptions;
using BotService.Application.Core;
using BotService.Infrastructure.Common.Logging;
using BotService.Infrastructure.Core;
using Domain.Enums;
using Gst;
using Gst.App;
using Microsoft.Extensions.Logging;
using System;
using static Domain.Constants.Constants;
using DateTime = System.DateTime;

namespace BotService.Infrastructure.Pipelines
{
    public class MediaInjectionPipeline : IObservable<BusEventPayload>, IMediaInjectionPipeline
    {
        private PipelineBusObserver pipelineBusObserver;
        private readonly ILogger logger;
        private readonly Pipeline pipeline;
        private readonly Bin audioProcessingBin, videoProcessingBin;
        private readonly AppSink videoAppSink, audioAppSink;
        private readonly MediaInjectionSettings injectionSettings;
        private readonly Bus bus;

        public MediaInjectionPipeline(
            MediaInjectionSettings injectionSettings,
            ILoggerFactory loggerFactory)
        {
            this.injectionSettings = injectionSettings;
            this.logger = loggerFactory.CreateLogger<MediaInjectionPipeline>();

            var name = $"pipeline_{injectionSettings.StreamId}";
            pipeline = new Pipeline(name);
            bus = pipeline.Bus;
            Element source = GetSource();
            Element decodebin = ElementFactory.Make("decodebin");

            if (source == null || decodebin == null)
            {
                throw new StartStreamInjectionException("Could not create all the pipeline elements");
            }

            decodebin.PadAdded += HandlePadAdded;
            videoProcessingBin = CreateVideoProcessingBin();
            audioProcessingBin = CreateAudioProcessingBin();

            audioAppSink = new AppSink("audioappsink")
            {
                EmitSignals = true
            };

            videoAppSink = new AppSink("videoappsink")
            {
                EmitSignals = true
            };

            pipeline.Add(source, decodebin);
            Element.Link(source, decodebin);
        }

        public (State State, State NextState) GetState()
        {
            pipeline.GetState(out State state, out State pending, 1000);

            return (state, pending);
        }

        public void Play()
        {
            bus.EnableSyncMessageEmission();
            bus.SyncMessage += OnBusMessage;
            pipeline.SetState(State.Playing);
            logger.LogInformation("[Media Injection] Started injection");
        }

        public void Stop()
        {
            pipeline.SetState(State.Null);
            bus.SyncMessage -= OnBusMessage;
            bus.Unref();
            pipeline.Unref();

            logger.LogInformation("[Media Injection] Stopped injection");
        }

        public void SetNewAudioSampleHandler(NewSampleHandler newAudioSampleHandler)
        {
            audioAppSink.NewSample += newAudioSampleHandler;
        }

        public void SetNewVideoSampleHandler(NewSampleHandler newVideoSampleHandler)
        {
            videoAppSink.NewSample += newVideoSampleHandler;
        }

        public void RemoveNewAudioSampleHandler(NewSampleHandler newAudioSampleHandler)
        {
            audioAppSink.NewSample -= newAudioSampleHandler;
        }

        public void RemoveNewVideoSampleHandler(NewSampleHandler newVideoSampleHandler)
        {
            videoAppSink.NewSample -= newVideoSampleHandler;
        }

        public IDisposable Subscribe(IObserver<BusEventPayload> observer)
        {
            // TODO: research how to subscribe to the observer best practices
            pipelineBusObserver = observer as PipelineBusObserver;

            return new PipelineBusObserverUnsuscriber(pipelineBusObserver);
        }

        private static void SetSrtParameters(Element srtSrc, SrtSettings srtSettings)
        {
            if (!string.IsNullOrEmpty(srtSettings.Passphrase))
            {
                srtSrc.SetProperty("passphrase", new GLib.Value(srtSettings.Passphrase));
            }

            if (srtSettings.Latency > 0)
            {
                srtSrc.SetProperty("latency", new GLib.Value(srtSettings.Latency));
            }
        }

        private static string GetRtmpLocation(RtmpSettings settings)
        {
            var isPushMode = settings.Mode == RtmpMode.Push;
            var uri = isPushMode ?
                GetRtmpPushLocation(settings) :
                GetRtmpPullLocation(settings);

            return uri;
        }

        private static string GetRtmpPushLocation(RtmpSettings settings)
        {
            return settings.EnableSsl ? string.Format(MediaInjectionUrl.Rtmps.Push.Gstreamer, "localhost", settings.StreamKey) : string.Format(MediaInjectionUrl.Rtmp.Push.Gstreamer, "localhost", settings.StreamKey);
        }

        private static string GetRtmpPullLocation(RtmpSettings settings)
        {
            return string.IsNullOrEmpty(settings.StreamKey) ? settings.StreamUrl : $"{settings.StreamUrl}/{settings.StreamKey}";
        }

        private static string GetSrtUri(SrtSettings settings)
        {
            var isListenerMode = settings.Mode == SrtMode.Listener;
            var uri = isListenerMode ? string.Format(MediaInjectionUrl.Srt.Listener.Gstreamer, string.Empty) : settings.Url;

            return uri;
        }

        private Element GetSource()
        {
            var protocolSettings = this.injectionSettings.ProtocolSettings;
            Element element;
            switch (protocolSettings.Type)
            {
                case Protocol.SRT:
                    var srtSettings = (SrtSettings)protocolSettings;
                    var uri = GetSrtUri(srtSettings);
                    element = ElementFactory.Make("srtsrc");
                    element.SetProperty("uri", new GLib.Value(uri));
                    SetSrtParameters(element, srtSettings);

                    break;
                case Protocol.RTMP:
                    element = ElementFactory.Make("rtmpsrc");
                    var location = GetRtmpLocation((RtmpSettings)protocolSettings);
                    element.SetProperty("location", new GLib.Value(location));

                    break;
                default:
                    throw new ArgumentException("Protocol not allowed for stream injection");
            }

            return element;
        }

        private Bin CreateVideoProcessingBin()
        {
            var videoBin = new Bin("videoprocessingbin");
            var queue = ElementFactory.Make("queue");
            var videoConvert = ElementFactory.Make("videoconvert");
            var videoScale = ElementFactory.Make("videoscale");
            var videoRate = ElementFactory.Make("videorate");
            var capsFilter = ElementFactory.Make("capsfilter");
            var caps = Caps.FromString("video/x-raw, format=NV12, height=1080, width=1920, pixel-aspect-ratio=1/1, framerate=30/1");

            videoRate.SetProperty("skip-to-first", new GLib.Value(true));
            capsFilter.SetProperty("caps", new GLib.Value(caps));
            videoBin.Add(queue, videoConvert, videoScale, videoRate, capsFilter);

            // Add sink and src bin pads to be connected with others elements
            AddBinPads(videoBin, queue, capsFilter);

            if (!Element.Link(queue, videoConvert, videoScale, videoRate, capsFilter))
            {
                logger.LogError("Could not link video processing elements for stream id: {streamId}", injectionSettings.StreamId);

                throw new StartStreamInjectionException("Error linking video processing elements");
            }

            return videoBin;
        }

        private Bin CreateAudioProcessingBin()
        {
            var audioBin = new Bin("AudioProcessingBin");
            var queue = ElementFactory.Make("queue");
            var audioConvert = ElementFactory.Make("audioconvert");
            var audioResample = ElementFactory.Make("audioresample");
            var audioBufferSplit = ElementFactory.Make("audiobuffersplit"); // delivers buffers with 20ms of audio
            var capsFilter = ElementFactory.Make("capsfilter");
            var caps = Caps.FromString("audio/x-raw, format=S16LE, channels=1, rate=16000"); // set capabilities needed for injection

            audioBufferSplit.SetProperty("strict-buffer-size", new GLib.Value(true));
            audioBufferSplit.SetProperty("discont-wait", new GLib.Value(20000000)); // waits 20ms before introducing discontinuities
            capsFilter.SetProperty("caps", new GLib.Value(caps));
            audioBin.Add(queue, audioConvert, audioResample, audioBufferSplit, capsFilter);

            // Add sink and src bin pads to be connected with others elements
            AddBinPads(audioBin, queue, capsFilter);

            if (!Element.Link(queue, audioConvert, audioResample, audioBufferSplit, capsFilter))
            {
                logger.LogError("Could not link audio processing elements for stream id: {streamId}", injectionSettings.StreamId);

                throw new StartStreamInjectionException("Error linking audio processing elements");
            }

            return audioBin;
        }

        private static void AddBinPads(Bin bin, Element sinkElement, Element srcElement)
        {
            var sinkPad = sinkElement.GetStaticPad("sink");
            var binSinkPad = new GhostPad("sink", sinkPad);
            var srcPad = srcElement.GetStaticPad("src");
            var binSrcPad = new GhostPad("src", srcPad);

            bin.AddPad(binSrcPad);
            bin.AddPad(binSinkPad);
        }

        private void OnBusMessage(object sender, GLib.SignalArgs args)
        {
            var msg = (Message)args.Args[0];
            (BusMessageType messageType, string formatedMessage) = GetFormatedMessage(msg);

            if (!string.IsNullOrEmpty(formatedMessage))
            {
                var busEvent = new BusEventPayload
                {
                    CallId = injectionSettings.CallId,
                    StreamId = injectionSettings.StreamId,
                    MessageType = messageType,
                    Message = formatedMessage,
                    DateTime = DateTime.Now
                };

                pipelineBusObserver.OnNext(busEvent);
            }
        }

        private void HandlePadAdded(object o, PadAddedArgs args)
        {
            var element = o as Element;
            var newPad = args.NewPad;
            // Check the new pad's type
            var newPadCaps = newPad.Caps;
            var newPadStruct = newPadCaps.GetStructure(0);
            var newPadType = newPadStruct.Name;

            if (newPadType.StartsWith("video"))
            {
                var linkStatus = LinkProcesingBin(videoProcessingBin, videoAppSink, newPad);

                if (linkStatus != PadLinkReturn.Ok)
                {
                    logger.LogError("Couldn't link video src pad from element {element} in stream {streamId}", element, injectionSettings.StreamId);
                }
            }
            else if (newPadType.StartsWith("audio"))
            {
                var linkStatus = LinkProcesingBin(audioProcessingBin, audioAppSink, newPad);

                if (linkStatus != PadLinkReturn.Ok)
                {
                    logger.LogError("Couldn't link audio src pad from element {element} in stream {streamId}", element, injectionSettings.StreamId);
                }
            }
        }

        private PadLinkReturn LinkProcesingBin(Bin bin, AppSink appSink, Pad decodePad)
        {
            var binSinkPad = bin.GetStaticPad("sink");
            var binSrcPad = bin.GetStaticPad("src");
            var appSinkPad = appSink.GetStaticPad("sink");

            pipeline.Add(bin, appSink);
            bin.SyncStateWithParent();
            appSink.SyncStateWithParent();
            binSrcPad.Link(appSinkPad);

            return decodePad.Link(binSinkPad);
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
                    formatedMessage = $"Error received from element {msg.Src.Name}: {err.Message}, Debugging information {(debug ?? "none")} {structure}";
                    break;
                case MessageType.Qos:
                    msg.ParseQosStats(out Gst.Format format, out ulong processed, out ulong dropped);
                    messageType = BusMessageType.Qos;
                    formatedMessage = $"QoS message from: {msg.Src.Name}, Format = {format}, Processed = {processed}, Dropped = {dropped}";
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
                case MessageType.Eos:
                    messageType = BusMessageType.Eos;
                    formatedMessage = "End of stream received";
                    break;
            }

            return (messageType, formatedMessage);
        }
    }
}
