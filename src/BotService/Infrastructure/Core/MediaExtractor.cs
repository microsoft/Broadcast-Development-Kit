using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using BotService.Application.Core;
using BotService.Infrastructure.Pipelines;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Skype.Bots.Media;

namespace BotService.Infrastructure.Core
{
    public class MediaExtractor : IMediaExtractor, IDisposable
    {
        private readonly IAudioSocket _audioSocket;
        private readonly IMediaProcessorFactory _mediaProcessorFactory;
        private readonly Timer _requestKeyFrameTimer;

        private IMediaProcessor _mediaProcessor;
        private bool _disposed = false;

        protected ILogger _logger;
        protected MediaExtractionSettings _mediaStreamSettings;

        public MediaExtractor(IVideoSocket videoSocket, IAudioSocket audioSocket, IMediaProcessorFactory mediaProcessorFactory, ILoggerFactory loggerFactory)
        {
            VideoSocket = videoSocket;
            _audioSocket = audioSocket;
            _mediaProcessorFactory = mediaProcessorFactory;

            _logger = loggerFactory.CreateLogger<MediaExtractor>();

            _requestKeyFrameTimer = new Timer(2000);
            _requestKeyFrameTimer.Elapsed += OnRequestKeyFrameTimer;
        }

        public Protocol Protocol => _mediaStreamSettings.ProtocolSettings.Type;

        public IVideoSocket VideoSocket { get; protected set; }

        public bool IsRunning { get; private set; } = false;

        public void Start(MediaExtractionSettings mediaStreamSettings)
        {
            try
            {
                _mediaStreamSettings = mediaStreamSettings;
                _mediaProcessor = _mediaProcessorFactory.CreateMediaProcessor(mediaStreamSettings.ProtocolSettings);
                _mediaProcessor.Play();

                VideoSocket.Subscribe(mediaStreamSettings.VideoResolution, mediaStreamSettings.MediaSourceId);
                VideoSocket.VideoMediaReceived += this.OnVideoMediaReceived;

                Task.Delay(250).Wait();

                // This socket is shared across all MediaSockets so we don't need to invoke the Subscribe method. 
                _audioSocket.AudioMediaReceived += this.OnAudioMediaReceived;
                
                _requestKeyFrameTimer.Start();

                IsRunning = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscribing to video failed for the socket: {SocketId}", VideoSocket.SocketId);
            }
        }

        public void Stop()
        {
            try
            {
                _mediaProcessor.Stop();
                _requestKeyFrameTimer.Stop();

                VideoSocket.Unsubscribe();
                VideoSocket.VideoMediaReceived -= this.OnVideoMediaReceived;

                // The audio socket is shared across all MediaSockets, so we should not invoke Unsubscribe on it.
                _audioSocket.AudioMediaReceived -= this.OnAudioMediaReceived;

                IsRunning = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unsubscribing to video failed for the socket: {SocketId}", VideoSocket.SocketId);
            }
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
                _requestKeyFrameTimer.Stop();

                _audioSocket.AudioMediaReceived -= this.OnAudioMediaReceived;

                VideoSocket.VideoMediaReceived -= this.OnVideoMediaReceived;
                VideoSocket.Unsubscribe();

                if (_mediaProcessor != null)
                {
                    _mediaProcessor.Dispose();
                }
            }

            _disposed = true;
        }

        protected virtual void OnAudioMediaReceived(object sender, AudioMediaReceivedEventArgs e)
        {
            var timestamp = e.Buffer.Timestamp;
            var audioFormat = e.Buffer.AudioFormat;
            var bytes = new byte[e.Buffer.Length];
            Marshal.Copy(e.Buffer.Data, bytes, 0, (int)e.Buffer.Length);
            e.Buffer.Dispose();

            try
            {
                _mediaProcessor.PushAudioBuffer(bytes, audioFormat, timestamp, 44100);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error pushing audio buffer");
                _logger.LogError("Error message {Message}, stack trace: {StackTrace}", ex.Message, ex.StackTrace);
            }
        }

        protected virtual void OnVideoMediaReceived(object sender, VideoMediaReceivedEventArgs e)
        {
            var timestamp = e.Buffer.Timestamp;
            var videoColorFormat = e.Buffer.VideoFormat.VideoColorFormat;
            var width = e.Buffer.VideoFormat.Width;
            var height = e.Buffer.VideoFormat.Height;
            var bytes = new byte[e.Buffer.Length];
            Marshal.Copy(e.Buffer.Data, bytes, 0, (int)e.Buffer.Length);
            e.Buffer.Dispose();

            try
            {
                _mediaProcessor.PushVideoBuffer(bytes, videoColorFormat, timestamp, width, height);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error pushing video buffer");
                _logger.LogError("Error message {Message}, stack trace: {StackTrace}", ex.Message, ex.StackTrace);
            }
        }

        private void OnRequestKeyFrameTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                VideoSocket.RequestKeyFrame();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error requesting a key frame for video socket {SocketId}", VideoSocket.SocketId);
            }
        }
    }
}
