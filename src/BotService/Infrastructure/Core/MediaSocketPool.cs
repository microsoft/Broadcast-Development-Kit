using System;
using System.Collections.Generic;
using System.Linq;
using BotService.Application.Core;
using Microsoft.Graph.Communications.Calls;
using Microsoft.Graph.Communications.Calls.Media;
using Microsoft.Skype.Bots.Media;

namespace BotService.Infrastructure.Core
{
    public class MediaSocketPool : IMediaSocketPool
    {
        private readonly object _lockObject = new object();

        // The Media Platform provides a separate socket for screen share, so we want to keep it separated from the rest.
        private readonly IVideoSocket _vbssSocket;
        private bool _isVbssSocketAvailable = true;

        // We are only configuring the first socket with the capability to send video, so we keep it apart.
        private readonly IVideoSocket _injectionSocket;
        private bool _isInjectionSocketAvailable = true;

        // These will be used to subscribe to the video feed of each participant.
        private readonly List<IVideoSocket> _freeParticipantSockets;
        private readonly List<IVideoSocket> _takenParticipantSockets;

        public MediaSocketPool(ICall call)
        {
            var mediaSession = call.GetLocalMediaSession();
            
            // Unfortunately, we cannot filter the video sockets by their properties after they are configured. However, we know that the first socket we create is the one supporting injection.
            _injectionSocket = mediaSession.VideoSocket;
            _vbssSocket = mediaSession.VbssSocket;
            _freeParticipantSockets = new List<IVideoSocket>(mediaSession.VideoSockets.Except(new[] { _injectionSocket }));
            _takenParticipantSockets = new List<IVideoSocket>();

            MainAudioSocket = mediaSession.AudioSocket;
        }

        // We have one audio socket for the call, with the audio form all the participants mixed.
        public IAudioSocket MainAudioSocket { get; private set; }

        public IVideoSocket GetScreenShareSocket()
        {
            lock (_lockObject)
            {
                if (!_isVbssSocketAvailable)
                {
                    // TODO: Throw custom exception
                    return null;
                }

                _isVbssSocketAvailable = false;

                return _vbssSocket;
            }
        }

        public IVideoSocket GetParticipantVideoSocket()
        {
            lock (_lockObject)
            {
                if (!_freeParticipantSockets.Any())
                {
                    return null;
                }

                var socket = _freeParticipantSockets.First();

                _freeParticipantSockets.Remove(socket);
                _takenParticipantSockets.Add(socket);

                return socket;
            }
        }


        public IVideoSocket GetInjectionVideoSocket()
        {
            lock (_lockObject)
            {
                if (!_isInjectionSocketAvailable)
                {
                    return null;
                }

                _isInjectionSocketAvailable = false;

                return _injectionSocket;
            }
        }

        public void ReleaseSocket(IVideoSocket socket)
        {
            lock (_lockObject)
            {
                // Is this the screens share socket?
                if (_vbssSocket == socket)
                {
                    _vbssSocket.Unsubscribe();
                    _isVbssSocketAvailable = true;
                }
                // Is this the injection video socket?
                else if (_injectionSocket == socket)
                {
                    _injectionSocket.Unsubscribe(); // Just in case
                    _isInjectionSocketAvailable = true;
                }
                // Then it must be a participant / injection socket
                else if (_takenParticipantSockets.Contains(socket))
                {
                    _takenParticipantSockets.Remove(socket);
                    _freeParticipantSockets.Add(socket);
                }
            }
        }
    }
}
