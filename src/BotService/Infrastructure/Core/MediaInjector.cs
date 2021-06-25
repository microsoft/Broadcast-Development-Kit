using System;
using System.Runtime.InteropServices;
using System.Threading;
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
    public class MediaInjector : IMediaInjector, IDisposable
    {
        private const int VideoSamplesPerSecond = 30;
        private const int AudioSamplesPerSecond = 50;

        private const ulong VideoSampleLength = 1000000000 / VideoSamplesPerSecond; // 33.33...ms, in nano-seconds
        private const ulong AudioSampleLength = 1000000000 / AudioSamplesPerSecond; // 20ms, in nano-seconds

        private readonly IAudioSocket _audioSocket;
        private readonly PipelineBusObserver _pipelineBusObserver;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MediaInjector> _logger;

        private readonly object _updateReferenceAudioLock = new object();
        private readonly object _updateReferenceVideoLock = new object();

        private IMediaInjectionPipeline _pipeline;
        private IDisposable _unsubscribeObserver;
        private bool _disposed = false;

        // Counters and flags for audio
        private int _audioReceivedSamples = 0;
        private int _audioAcceptedSamples = 0;
        private int _audioDroppedSamples = 0;

        private ulong _referenceAudioPts;
        private long _referenceAudioTimestamp;
        private bool _isAudioReady = false;

        // Counters and flags for video
        private int _videoReceivedFrames = 0;
        private int _videoAcceptedFrames = 0;
        private int _videoDroppedFrames = 0;
        private ulong _referenceVideoPts;
        private long _referenceVideoTimestamp;
        private bool _isVideoReady = false;

        public MediaInjector(IVideoSocket videoSocket, IAudioSocket audioSocket, ILoggerFactory loggerFactory, PipelineBusObserver pipelineBusObserver)
        {
            VideoSocket = videoSocket;
            _audioSocket = audioSocket;
            _pipelineBusObserver = pipelineBusObserver;

            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MediaInjector>();
        }

        public IVideoSocket VideoSocket { get; protected set; }

        public void Start(MediaInjectionSettings injectionSettings)
        {
            // Stop previous injection if it was not stoped
            StopPreviousInjectionStarted();

            _pipeline = new MediaInjectionPipeline(injectionSettings, _loggerFactory);
            _pipeline.SetNewAudioSampleHandler(NewAudioSample);
            _pipeline.SetNewVideoSampleHandler(NewVideoSample);
            _unsubscribeObserver = _pipeline.Subscribe(_pipelineBusObserver);

            _pipeline.Play();
        }

        public void Stop()
        {
            _pipeline.RemoveNewAudioSampleHandler(NewAudioSample);
            _pipeline.RemoveNewVideoSampleHandler(NewVideoSample);
            _pipeline.Stop();

            _isAudioReady = false;
            _isVideoReady = false;

            // Unsubscribe observer
            _unsubscribeObserver?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Stop previous injection if it was not stoped
                StopPreviousInjectionStarted();
            }

            _disposed = true;
        }

        private static IntPtr CopyBufferContents(Gst.Buffer buffer, out int size)
        {
            buffer.Map(out MapInfo info, MapFlags.Read);
            size = (int)info.Size;
            IntPtr contentPtr = Marshal.AllocHGlobal(size);

            unsafe
            {
                System.Buffer.MemoryCopy(info.DataPtr.ToPointer(), contentPtr.ToPointer(), size, size);
            }

            buffer.Unmap(info);

            return contentPtr;
        }

        private static bool TryGetPipelineTime(AppSink sink, ref ulong runningTime)
        {
            Gst.Clock clock = sink.Clock;

            if (clock != null)
            {
                runningTime = clock.Time - sink.BaseTime;
                clock.Dispose();

                return true;
            }

            return false;
        }

        private void NewVideoSample(object sender, GLib.SignalArgs args)
        {
            var sink = (AppSink)sender;
            var sample = sink.PullSample();

            if (sample != null)
            {
                Interlocked.Increment(ref _videoReceivedFrames);

                ulong pipelineTime = 0;
                if (TryGetPipelineTime(sink, ref pipelineTime))
                {
                    Gst.Buffer buffer = sample.Buffer;
                    ulong adjustedBufferPts = buffer.Pts + sink.Latency;

                    // If this is the first sample we got in this injection, then we take the current pipeline PTS as a reference for the following samples.
                    // To avoid adding overhead with the lock statement fist we do a dirty read.
                    if (!_isVideoReady)
                    {
                        lock (_updateReferenceVideoLock)
                        {
                            // Now that we are inside the lock statement we need to check the condition again.
                            if (!_isVideoReady)
                            {
                                _referenceVideoPts = pipelineTime;
                                _referenceVideoTimestamp = MediaPlatform.GetCurrentTimestamp();
                                _isVideoReady = true;

                                _logger.LogInformation($"[Injection] Video - Setting reference at pipeline PTS: {_referenceVideoPts}(ns) and platform timestamp: {_referenceVideoTimestamp}(100-ns).");
                                _logger.LogInformation($"[Injection] Video - Current pipeline latency: {sink.Latency}");
                            }
                        }
                    }

                    // If the frame is too late we will simply discard it
                    if (adjustedBufferPts + VideoSampleLength >= pipelineTime)
                    {
                        // The Media Platform uses timestamps in units of 100-ns, while GStreamer uses timestamps in units of 1-ns.
                        long timestamp = _referenceVideoTimestamp + ((long)(adjustedBufferPts - _referenceVideoPts) / 100);
                        long platformTimestamp = MediaPlatform.GetCurrentTimestamp();
                        long timestampDrift = platformTimestamp - timestamp;

                        if (_videoReceivedFrames % VideoSamplesPerSecond == 0)
                        {
                            _logger.LogInformation($"[Injection] Video - Frames Received: {_videoReceivedFrames - 1} Accepted: {_videoAcceptedFrames} Dropped: {_videoDroppedFrames}");
                            _logger.LogInformation($"[Injection] Video - Buffer #{_videoReceivedFrames - 1} check - Pipeline time: {pipelineTime}(ns) Buffer PTS: {buffer.Pts}(ns) Latency: {sink.Latency}(ns) Platform diff: {timestampDrift}(100-ns)");
                        }
                        else if (_videoReceivedFrames == 1)
                        {
                            _logger.LogInformation($"[Injection] Video - First video frame received with PTS: {buffer.Pts}(ns) and latency: {sink.Latency}(ns).");
                        }

                        // This method allocates unmanaged memory to copy the data in the buffer
                        IntPtr contentPtr = CopyBufferContents(buffer, out int size);
                        var videoSendBuffer = new VideoSendBuffer(contentPtr, size, VideoFormat.NV12_1920x1080_30Fps, timestamp);

                        try
                        {
                            VideoSocket.Send(videoSendBuffer);
                            Interlocked.Increment(ref _videoAcceptedFrames);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[Injection] Video - Error sending buffer - Message: {message}", ex.InnerException?.Message ?? ex.Message);
                            Interlocked.Increment(ref _videoDroppedFrames);
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"[Injection] Video - Frame arrived too late, discarding frame. Buffer PTS (with Latency): {adjustedBufferPts}(ns) Pipeline PTS: {pipelineTime}(ns)");
                        Interlocked.Increment(ref _videoDroppedFrames);
                    }

                    // We need to dispose the sample after we are done with it
                    buffer.Dispose();
                }
                else
                {
                    _logger.LogWarning($"[Injection] Video - Clock is null. Discarding frame.");
                    Interlocked.Increment(ref _videoDroppedFrames);
                }

                sample.Dispose();
            }
            else
            {
                _logger.LogWarning("[Injection] Video - New sample signal was triggered without a sample.");
            }
        }

        private void NewAudioSample(object sender, GLib.SignalArgs args)
        {
            var sink = (AppSink)sender;
            var sample = sink.PullSample();

            if (sample != null)
            {
                Interlocked.Increment(ref _audioReceivedSamples);

                ulong pipelineTime = 0;
                if (TryGetPipelineTime(sink, ref pipelineTime))
                {
                    Gst.Buffer buffer = sample.Buffer;
                    ulong adjustedBufferPts = buffer.Pts + sink.Latency;

                    // If this is the first sample we got in this injection, then we take the current pipeline PTS as a reference for the following samples.
                    // To avoid adding overhead with the lock statement fist we do a dirty read.
                    if (!_isAudioReady)
                    {
                        lock (_updateReferenceAudioLock)
                        {
                            // Now that we are inside the lock statement we need to check the condition again.
                            if (!_isAudioReady)
                            {
                                _referenceAudioPts = pipelineTime;
                                _referenceAudioTimestamp = MediaPlatform.GetCurrentTimestamp();
                                _isAudioReady = true;

                                _logger.LogInformation($"[Injection] Audio - Setting reference at pipeline PTS: {_referenceAudioPts}(ns) and platform timestamp: {_referenceAudioTimestamp}(100-ns).");
                                _logger.LogInformation($"[Injection] Audio - Current pipeline latency: {sink.Latency}");
                            }
                        }
                    }

                    // If the audio sample is too late we will simply discard it
                    if (adjustedBufferPts + AudioSampleLength >= pipelineTime)
                    {
                        // The Media Platform uses timestamps in units of 100-ns, while GStreamer uses timestamps in units of 1-ns.
                        long timestamp = _referenceAudioTimestamp + ((long)(adjustedBufferPts - _referenceAudioPts) / 100);
                        long platformTimestamp = MediaPlatform.GetCurrentTimestamp();
                        long timestampDrift = platformTimestamp - timestamp;

                        if (_audioReceivedSamples % AudioSamplesPerSecond == 0)
                        {
                            _logger.LogInformation($"[Injection] Audio - Samples Received: {_audioReceivedSamples - 1} Accepted: {_audioAcceptedSamples} Dropped: {_audioDroppedSamples}");
                            _logger.LogInformation($"[Injection] Audio - Buffer #{_audioReceivedSamples - 1} check - Pipeline time: {pipelineTime}(ns) Buffer PTS: {buffer.Pts}(ns) Latency: {sink.Latency}(ns) Platform diff: {timestampDrift}(100-ns)");
                        }
                        else if (_audioReceivedSamples == 1)
                        {
                            _logger.LogInformation($"[Injection] Audio - First audio sample received with PTS: {buffer.Pts}(ns) and latency: {sink.Latency}(ns).");
                        }

                        // This method allocates unmanaged memory to copy the data in the buffer
                        IntPtr contentPtr = CopyBufferContents(buffer, out int size);
                        var audioSendBuffer = new AudioSendBuffer(contentPtr, size, AudioFormat.Pcm16K, timestamp);

                        try
                        {
                            _audioSocket.Send(audioSendBuffer);
                            Interlocked.Increment(ref _audioAcceptedSamples);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("[Injection] Audio - Error sending buffer - Message: {message}", ex.InnerException.Message);
                            Interlocked.Increment(ref _audioDroppedSamples);
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"[Injection] Audio - Sample arrived too late, discarding sample. Buffer PTS (with Latency): {adjustedBufferPts}(ns) Pipeline PTS: {pipelineTime}(ns)");
                        Interlocked.Increment(ref _audioDroppedSamples);
                    }

                    // We need to dispose the buffer after we are done with it
                    buffer.Dispose();
                }
                else
                {
                    _logger.LogWarning($"[Injection] Audio - Clock is null. Discarding sample.");
                    Interlocked.Increment(ref _audioDroppedSamples);
                }

                sample.Dispose();
            }
            else
            {
                _logger.LogWarning("[Injection] Audio - New sample signal was triggered without a sample.");
            }
        }

        private void StopPreviousInjectionStarted()
        {
            if (_pipeline != null)
            {
                (State state, _) = _pipeline.GetState();
                if (state != State.Null)
                {
                    Stop();
                }
            }
        }
    }
}
