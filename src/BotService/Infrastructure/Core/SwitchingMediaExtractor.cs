using System;
using BotService.Application.Core;
using BotService.Infrastructure.Pipelines;
using Microsoft.Extensions.Logging;
using Microsoft.Skype.Bots.Media;

namespace BotService.Infrastructure.Core
{
    public class SwitchingMediaExtractor : MediaExtractor, ISwitchingMediaExtractor
    {
        private readonly object _switchLock = new object();
        private readonly IMediaSocketPool _mediaSocketPool;

        private IVideoSocket _nextVideoSocket;

        public SwitchingMediaExtractor(IVideoSocket initialVideoSocket, IMediaSocketPool mediaSocketPool, IAudioSocket audioSocket, IMediaProcessorFactory mediaProcessorFactory, ILoggerFactory loggerFactory)
            : base(initialVideoSocket, audioSocket, mediaProcessorFactory, loggerFactory)
        {
            _mediaSocketPool = mediaSocketPool;
            _logger = loggerFactory.CreateLogger<SwitchingMediaExtractor>();
        }

        public void SwitchMediaSourceSafely(uint mediaSourceId)
        {
            if (IsRunning)
            {
                try
                {
                    if (_nextVideoSocket != null)
                    {
                        _nextVideoSocket.VideoMediaReceived -= OnVideoMediaReceived;
                        _nextVideoSocket.Unsubscribe();
                        _mediaSocketPool.ReleaseSocket(_nextVideoSocket);
                    }

                    // Attempt to subscribe a new socket temporarily 
                    var newSocket = _mediaSocketPool.GetParticipantVideoSocket();
                    if (newSocket != null)
                    {
                        // We subscribe to the new socket and inmediately request a key-frame
                        newSocket.Subscribe(_mediaStreamSettings.VideoResolution, mediaSourceId);
                        newSocket.VideoMediaReceived += OnVideoMediaReceived;
                        newSocket.RequestKeyFrame();

                        _nextVideoSocket = newSocket;
                    }
                    else
                    {
                        // We don't have any available sockets. Force a dirty switch with our current socket.
                        SwitchMediaSourceForcefully(mediaSourceId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There was an error trying to initiate a switch between video sockets");
                }
            }
        }

        public void SwitchMediaSourceForcefully(uint mediaSourceId)
        {
            VideoSocket.Subscribe(_mediaStreamSettings.VideoResolution, mediaSourceId);
        }

        protected override void OnVideoMediaReceived(object sender, VideoMediaReceivedEventArgs e)
        {
            if (_nextVideoSocket == null)
            {
                if (e.SocketId == VideoSocket.SocketId)
                {
                    base.OnVideoMediaReceived(sender, e);
                }
            }
            else
            {
                lock (_switchLock)
                {
                    if (e.SocketId == _nextVideoSocket.SocketId)
                    {
                        if (H264Helper.IsKeyFrame(e.Buffer.Data, e.Buffer.Length, limit: 100))
                        {
                            // We are ready to switch to the new video socket. Drop the current socket.
                            var oldVideoSocket = VideoSocket;
                            VideoSocket = _nextVideoSocket;
                            _nextVideoSocket = null;

                            oldVideoSocket.Unsubscribe();
                            oldVideoSocket.VideoMediaReceived -= OnVideoMediaReceived;

                            _mediaSocketPool.ReleaseSocket(oldVideoSocket);

                            base.OnVideoMediaReceived(sender, e);
                        }
                    }
                    else if (e.SocketId == VideoSocket.SocketId)
                    {
                        base.OnVideoMediaReceived(sender, e);
                    }
                }
            }
        }
    }
}
