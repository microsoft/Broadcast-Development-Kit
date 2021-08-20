// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Application.Common.Config;
using Application.Common.Models;
using Application.Exceptions;
using BotService.Application.Core;
using BotService.Infrastructure.Common;
using BotService.Infrastructure.Extensions;
using BotService.Infrastructure.Pipelines;
using BotService.Infrastructure.Services;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Calls;
using Microsoft.Graph.Communications.Resources;
using Microsoft.Skype.Bots.Media;
using Task = System.Threading.Tasks.Task;

namespace BotService.Infrastructure.Core
{
    public class CallHandler : HeartbeatHandler
    {
        /// <summary>
        /// MSI when there is no primary speaker.
        /// </summary>
        private const uint PrimarySpeakerNone = DominantSpeakerChangedEventArgs.None;
        private const int DefaultSrtPort = 8888;
        private const int DefaultRtmpPort = 1940;
        private const int DefaultRtmpsPort = 2940;

        private readonly ILogger _logger;
        private readonly object _subscriptionLock = new object();

        private readonly ConcurrentDictionary<string, IMediaExtractor> _currentMediaExtractors = new ConcurrentDictionary<string, IMediaExtractor>();
        private readonly IMediaSocketPool _mediaSocketPool;

        private readonly int _numberOfMultiviewSockets;

        private readonly ConcurrentBag<int> _availablePorts;
        private readonly ConcurrentBag<int> _availableRtmpPorts;
        private readonly ConcurrentBag<int> _availableRtmpsPorts;
        private readonly ConcurrentDictionary<string, int> _currentAssignedPorts = new ConcurrentDictionary<string, int>();

        private readonly BotConfiguration _config;
        private readonly IMediatorService _mediatorService;
        private readonly IMediaHandlerFactory _mediaHandlerFactory;
        private readonly GstreamerClockProvider _clockProvider;

        private ISwitchingMediaExtractor _screenShareMediaSocket;
        private IMediaInjector _mediaInjector;
        private IMediaInjector _mediaSlateInjector;

        private bool _isCapturingScreenShare;
        private bool _isCapturingPrimarySpeaker;

        private uint _currentPrimarySpeaker = DominantSpeakerChangedEventArgs.None;
        private string _primarySpeakerId;

        public CallHandler(
            ICall statefulCall,
            BotConfiguration config,
            IMediatorService mediatorService,
            IMediaHandlerFactory mediaHandlerFactory,
            ILoggerFactory loggerFactory,
            GstreamerClockProvider clockProvider)
            : base(TimeSpan.FromMinutes(10), statefulCall?.GraphLogger)
        {
            _config = config;
            _mediatorService = mediatorService;
            _mediaHandlerFactory = mediaHandlerFactory;
            _logger = loggerFactory.CreateLogger<CallHandler>();
            _clockProvider = clockProvider;

            _numberOfMultiviewSockets = config.NumberOfMultiviewSockets;
            _availablePorts = new ConcurrentBag<int>(GetPorts(_numberOfMultiviewSockets, DefaultSrtPort));
            _availableRtmpPorts = new ConcurrentBag<int>(GetPorts(_numberOfMultiviewSockets, DefaultRtmpPort));
            _availableRtmpsPorts = new ConcurrentBag<int>(GetPorts(_numberOfMultiviewSockets, DefaultRtmpsPort));

            Call = statefulCall;
            Call.OnUpdated += CallOnUpdated;

            // Subscribe to the participants updates, this will inform the bot if a particpant left/joined the conference
            Call.Participants.OnUpdated += ParticipantsOnUpdated;

            _mediaSocketPool = new MediaSocketPool(statefulCall);
            _mediaSocketPool.MainAudioSocket.DominantSpeakerChanged += OnDominantSpeakerChanged;
        }

        /// <summary>
        /// Gets the call.
        /// </summary>
        public ICall Call { get; }

        public StartStreamExtractionResponse StartExtraction(StartStreamExtractionBody streamExtraction)
        {
            StartStreamExtractionResponse response;
            switch (streamExtraction.ResourceType)
            {
                case ResourceType.Participant:
                case ResourceType.LargeGallery:
                case ResourceType.TogetherMode:
                case ResourceType.LiveEvent:
                    response = StartParticipantStreamExtraction(streamExtraction);
                    break;
                case ResourceType.PrimarySpeaker:
                    response = StartPrimarySpeakerStreamExtraction(streamExtraction);
                    break;
                case ResourceType.Vbss:
                    response = StartScreenShareStreamExtraction(streamExtraction);
                    break;
                default:
                    throw new ArgumentException("The resource type to start is not supported.", nameof(streamExtraction));
            }

            return response;
        }

        public void StopExtraction(StopStreamExtractionBody streamExtraction)
        {
            switch (streamExtraction.ResourceType)
            {
                case ResourceType.Participant:
                case ResourceType.LargeGallery:
                case ResourceType.TogetherMode:
                case ResourceType.LiveEvent:
                    StopParticipantStreamExtraction(streamExtraction);
                    break;
                case ResourceType.PrimarySpeaker:
                    StopPrimarySpeakerStreamExtraction();
                    break;
                case ResourceType.Vbss:
                    StopScreenShareStreamExtraction(streamExtraction);
                    break;
                default:
                    throw new ArgumentException("The resource type to stop is not supported.", nameof(streamExtraction));
            }
        }

        public void StartInjection(StartStreamInjectionBody startStreamInjectionBody)
        {
            StopSlateInjection();

            var videoSocket = _mediaSocketPool.GetInjectionVideoSocket();
            if (videoSocket == null)
            {
                _logger.LogWarning("[Call Handler] There are no more video sockets available in the media session");

                throw new StartStreamInjectionException("There are no more video sockets available for this operation");
            }

            var injectionSettings = GetInjectionSettings(startStreamInjectionBody);

            _mediaInjector = _mediaHandlerFactory.CreateInjector(videoSocket, _mediaSocketPool.MainAudioSocket);
            _mediaInjector.Start(injectionSettings);
        }

        public void StopActiveStreams()
        {
            if (_mediaInjector != null)
            {
                _mediaInjector.Stop();
                _mediaSocketPool.ReleaseSocket(_mediaInjector.VideoSocket);
                _mediaInjector = null;
            }

            if (_mediaSlateInjector != null)
            {
                _mediaSlateInjector.Stop();
                _mediaSocketPool.ReleaseSocket(_mediaSlateInjector.VideoSocket);
                _mediaSlateInjector = null;
            }

            if (_currentMediaExtractors.Count > 0)
            {
                foreach (var item in _currentMediaExtractors)
                {
                    var participantGraphId = item.Key;
                    var mediaExtractor = item.Value;

                    mediaExtractor.Stop();
                    RemoveAssignedExtractionPort(participantGraphId, mediaExtractor.Protocol);
                    _mediaSocketPool.ReleaseSocket(mediaExtractor.VideoSocket);
                }
            }

            if (_screenShareMediaSocket != null)
            {
                _screenShareMediaSocket.Stop();
                _mediaSocketPool.ReleaseSocket(_screenShareMediaSocket.VideoSocket);
                _screenShareMediaSocket = null;

                if (_currentAssignedPorts.TryRemove(_currentAssignedPorts.First().Key, out int assignedPort))
                {
                    _availablePorts.Add(assignedPort);
                }
            }
        }

        public void StopInjection()
        {
            if (_mediaInjector != null)
            {
                _mediaInjector.Stop();
                _mediaSocketPool.ReleaseSocket(_mediaInjector.VideoSocket);
                _mediaInjector = null;
            }

            StartSlateInjection();
        }

        /// <inheritdoc/>
        protected override Task HeartbeatAsync(ElapsedEventArgs args) => Call.KeepAliveAsync();

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _clockProvider.ResetBaseTime();
            _mediaSocketPool.MainAudioSocket.DominantSpeakerChanged -= OnDominantSpeakerChanged;
            Call.OnUpdated -= CallOnUpdated;
            Call.Participants.OnUpdated -= ParticipantsOnUpdated;

            foreach (var participant in Call.Participants)
            {
                participant.OnUpdated -= OnParticipantUpdated;
            }
        }

        #region Private Methods
        private static IList<int> GetPorts(int numberOfSockets, int defaultPort)
        {
            List<int> ports = new List<int>();

            for (var i = 0; i < numberOfSockets + 1; i++)
            {
                var port = defaultPort + i;
                ports.Add(port);
            }

            return ports;
        }

        private static MediaInjectionSettings GetInjectionSettings(StartStreamInjectionBody streamInjectionBody)
        {
            ProtocolSettings protocolSettings;
            switch (streamInjectionBody.Protocol)
            {
                case Protocol.SRT:
                    var srtStreamBody = (SrtStreamInjectionBody)streamInjectionBody;
                    protocolSettings = new SrtSettings
                    {
                        Latency = srtStreamBody.Latency,
                        Mode = srtStreamBody.Mode,
                        Passphrase = srtStreamBody.StreamKey,
                        KeyLength = srtStreamBody.KeyLength,
                        Url = srtStreamBody.StreamUrl,
                    };

                    break;
                case Protocol.RTMP:
                    var rtmpStreamBody = (RtmpStreamInjectionBody)streamInjectionBody;
                    protocolSettings = new RtmpSettings
                    {
                        StreamUrl = rtmpStreamBody.StreamUrl,
                        StreamKey = rtmpStreamBody.StreamKey,
                        EnableSsl = rtmpStreamBody.EnableSsl,
                        Mode = rtmpStreamBody.Mode,
                    };
                    break;
                default:
                    throw new ArgumentException("Protocol not supported", nameof(streamInjectionBody));
            }

            var injectionSettings = new MediaInjectionSettings
            {
                CallId = streamInjectionBody.CallId,
                StreamId = streamInjectionBody.StreamId,
                ProtocolSettings = protocolSettings,
            };

            return injectionSettings;
        }

        private static StartStreamExtractionResponse GetStreamExtractionResponse(ProtocolSettings protocolSettings)
        {
            switch (protocolSettings.Type)
            {
                case Protocol.SRT:
                    var srtSettings = (SrtSettings)protocolSettings;

                    StartStreamExtractionResponse srtResponse = new StartSrtStreamExtractionResponse
                    {
                        Mode = srtSettings.Mode,
                        Protocol = Protocol.SRT,
                        Url = srtSettings.Url,
                        Port = srtSettings.Port,
                        Latency = srtSettings.Latency,
                        Passphrase = srtSettings.Passphrase,
                        KeyLength = srtSettings.KeyLength,
                        AudioFormat = srtSettings.AudioFormat,
                        TimeOverlay = srtSettings.TimeOverlay,
                    };

                    return srtResponse;
                case Protocol.RTMP:
                    var rtmpSettings = (RtmpSettings)protocolSettings;

                    StartStreamExtractionResponse rtmpResponse = new StartRtmpStreamExtractionResponse
                    {
                        Protocol = Protocol.RTMP,
                        Mode = rtmpSettings.Mode,
                        EnableSsl = rtmpSettings.EnableSsl,
                        StreamKey = rtmpSettings.StreamKey,
                        StreamUrl = rtmpSettings.StreamUrl,
                        Port = rtmpSettings.Port,
                        AudioFormat = rtmpSettings.AudioFormat,
                        TimeOverlay = rtmpSettings.TimeOverlay,
                    };

                    return rtmpResponse;
                default:
                    throw new ArgumentException("Protocol not supported", nameof(protocolSettings));
            }
        }

        private int AssignExtractionPort(StartStreamExtractionBody streamBody)
        {
            int port = 0;

            switch (streamBody.Protocol)
            {
                case Protocol.SRT:
                    if (!_availablePorts.TryTake(out int srtPort))
                    {
                        _logger.LogWarning("[Call Handler] There are no SRT ports available");

                        throw new StartStreamExtractionException("There are no SRT ports available");
                    }

                    port = srtPort;
                    break;
                case Protocol.RTMP:
                    var rtmpStreamBody = (RtmpStreamExtractionBody)streamBody;
                    int rtmpPort = 0;

                    if (rtmpStreamBody.EnableSsl)
                    {
                        if (!_availableRtmpsPorts.TryTake(out int rtmps))
                        {
                            _logger.LogWarning("[Call Handler] There are no RTMPS ports available");

                            throw new StartStreamExtractionException("There are no RTMPS ports available");
                        }

                        rtmpPort = rtmps;
                    }
                    else
                    {
                        if (!_availableRtmpPorts.TryTake(out int rtmp))
                        {
                            _logger.LogWarning("[Call Handler] There are no RTMP ports available");

                            throw new StartStreamExtractionException("There are no RTMP ports available");
                        }

                        rtmpPort = rtmp;
                    }

                    port = rtmpPort;
                    break;
                default:
                    throw new ArgumentException("Protocol not supported", nameof(streamBody));
            }

            _currentAssignedPorts.AddOrUpdate(streamBody.ParticipantGraphId, port, (k, v) => port);

            return port;
        }

        private void RemoveAssignedExtractionPort(string streamId, Protocol protocol)
        {
            _currentAssignedPorts.TryRemove(streamId, out int port);

            switch (protocol)
            {
                case Protocol.SRT:
                    _availablePorts.Add(port);
                    break;
                case Protocol.RTMP:
                    if (port >= DefaultRtmpPort && port <= (DefaultRtmpPort + _numberOfMultiviewSockets))
                    {
                        _availableRtmpPorts.Add(port);
                    }
                    else
                    {
                        _availableRtmpsPorts.Add(port);
                    }

                    break;
                default:
                    throw new ArgumentException("Protocol not supported", protocol.ToString());
            }
        }

        private StartStreamExtractionResponse StartParticipantStreamExtraction(StartStreamExtractionBody streamBody)
        {
            var participant = Call.Participants.FirstOrDefault(x => x.Id == streamBody.ParticipantGraphId);

            if (participant == null)
            {
                _logger.LogError("[Call Handler] Participant {participant} was not found", streamBody.ParticipantGraphId);
                throw new StartStreamExtractionException($"Participant {streamBody.ParticipantGraphId} was not found");
            }

            if (!participant.IsParticipantCapableToSendVideo())
            {
                _logger.LogError("[Call Handler] Participant {participant} has no send capable video stream", streamBody.ParticipantGraphId);
                throw new StartStreamExtractionException($"Participant {streamBody.ParticipantGraphId} has no send capable video stream");
            }

            var videoMediaStream = participant.GetParticipantStream();

            var msi = uint.Parse(videoMediaStream.SourceId);

            if (_currentMediaExtractors.ContainsKey(streamBody.ParticipantGraphId))
            {
                _logger.LogWarning("[Call Handler] Participant {participantId} already has a media stream assigned", streamBody.ParticipantGraphId);
                throw new StartStreamExtractionException($"Participant {streamBody.ParticipantGraphId} already has a media stream assigned");
            }

            var videoSocket = _mediaSocketPool.GetParticipantVideoSocket();
            if (videoSocket == null)
            {
                _logger.LogWarning("[Call Handler] There are no more video sockets available in the media session");
                throw new StartStreamExtractionException("There are no more video sockets available in the media session");
            }

            IMediaExtractor mediaExtractor = _mediaHandlerFactory.CreateExtractor(videoSocket, _mediaSocketPool.MainAudioSocket);
            MediaExtractionSettings mediaStreamSettings;
            lock (_subscriptionLock)
            {
                mediaStreamSettings = GetMediaStreamSettings(msi, streamBody);
                mediaExtractor.Start(mediaStreamSettings);
                _currentMediaExtractors.AddOrUpdate(streamBody.ParticipantGraphId, mediaExtractor, (k, v) => mediaExtractor);
            }

            StartStreamExtractionResponse response = GetStreamExtractionResponse(mediaStreamSettings.ProtocolSettings);

            return response;
        }

        private void StopParticipantStreamExtraction(StopStreamExtractionBody streamBody)
        {
            var participant = Call.Participants.FirstOrDefault(x => x.Id == streamBody.ParticipantGraphId);

            if (participant == null)
            {
                _logger.LogError("[Call Handler] Participant {participant} was not found", streamBody.ParticipantGraphId);
                throw new StopStreamExtractionException($"Participant {streamBody.ParticipantGraphId} was not found");
            }

            if (!_currentMediaExtractors.ContainsKey(streamBody.ParticipantGraphId))
            {
                _logger.LogWarning("[Call Handler] Participant {participantId} does not have an active stream", streamBody.ParticipantGraphId);
                throw new StopStreamExtractionException($"Participant {streamBody.ParticipantGraphId} does not have an active stream");
            }

            // TODO: I don't think we need this lock here
            lock (_subscriptionLock)
            {
                if (_currentMediaExtractors.TryRemove(streamBody.ParticipantGraphId, out IMediaExtractor mediaExtractor))
                {
                    mediaExtractor.Stop();

                    RemoveAssignedExtractionPort(streamBody.ParticipantGraphId, mediaExtractor.Protocol);

                    _mediaSocketPool.ReleaseSocket(mediaExtractor.VideoSocket);
                }
                else
                {
                    throw new StopStreamExtractionException("No extraction found for this participant");
                }
            }
        }

        private StartStreamExtractionResponse StartPrimarySpeakerStreamExtraction(StartStreamExtractionBody streamExtraction)
        {
            // validate that request.ParticipantId is the actual primary (a.k.a dominant) speaker participantstream of the call
            // OR make a method to find the primary speaker for the call
            _primarySpeakerId = streamExtraction.ParticipantGraphId;

            IParticipant participant;
            if (_currentPrimarySpeaker == PrimarySpeakerNone)
            {
                participant = Call.Participants.FirstOrDefault(x => x.IsParticipantCapableToSendVideo());
                if (participant == null)
                {
                    _logger.LogWarning("[Call Handler] There are not participants sharing video.");
                    throw new StartStreamExtractionException("There are not participants sharing video.");
                }
            }
            else
            {
                participant = GetParticipantFromMSI(_currentPrimarySpeaker);
                if (!participant.IsParticipantCapableToSendVideo())
                {
                    // TODO: Do something else
                    _logger.LogWarning("[Call Handler] Current primary speaker {id} cannot share video", participant.Resource.Id);
                    _logger.LogWarning("[Call Handler] Trying to get default participant");

                    participant = Call.Participants.FirstOrDefault(x => x.IsParticipantCapableToSendVideo());

                    if (participant == null)
                    {
                        _logger.LogWarning("[Call Handler] There are not participants sharing video.");
                        throw new StartStreamExtractionException("There are not participants sharing video.");
                    }
                }
            }

            var videoMediaStream = participant.GetParticipantStream();

            var msi = uint.Parse(videoMediaStream.SourceId);
            _currentPrimarySpeaker = msi;

            var mediaStreamSettings = GetMediaStreamSettings(msi, streamExtraction);

            if (!_currentMediaExtractors.TryGetValue(streamExtraction.ParticipantGraphId, out IMediaExtractor mediaExtractor))
            {
                IVideoSocket videoSocket = _mediaSocketPool.GetParticipantVideoSocket();

                if (videoSocket == null)
                {
                    _logger.LogWarning("[Call Handler] There are not media streams available");
                    throw new StartStreamExtractionException("There are not media streams available");
                }

                mediaExtractor = _mediaHandlerFactory.CreateSwitchingExtractor(videoSocket, _mediaSocketPool, _mediaSocketPool.MainAudioSocket);
                _currentMediaExtractors.TryAdd(streamExtraction.ParticipantGraphId, mediaExtractor);
            }

            mediaExtractor.Start(mediaStreamSettings);

            _isCapturingPrimarySpeaker = true;

            StartStreamExtractionResponse response = GetStreamExtractionResponse(mediaStreamSettings.ProtocolSettings);

            return response;
        }

        private void StopPrimarySpeakerStreamExtraction()
        {
            // TODO: Review if this lock is required.
            lock (_subscriptionLock)
            {
                _isCapturingPrimarySpeaker = false;
                if (_currentMediaExtractors.TryRemove(_primarySpeakerId, out IMediaExtractor mediaExtractor))
                {
                    mediaExtractor.Stop();

                    RemoveAssignedExtractionPort(_primarySpeakerId, mediaExtractor.Protocol); // TODO: Verify if necessary to validate this case

                    _mediaSocketPool.ReleaseSocket(mediaExtractor.VideoSocket);
                }
                else
                {
                    throw new StopStreamExtractionException("No extraction found for the primary speaker");
                }
            }
        }

        private StartStreamExtractionResponse StartScreenShareStreamExtraction(StartStreamExtractionBody streamExtraction)
        {
            var vbssParticipant = Call.Participants.FirstOrDefault(p => p.IsParticipantSharingScreen());

            if (vbssParticipant == null)
            {
                _logger.LogError("[Call Handler] Participant {participant} is not sharing screen", streamExtraction.ParticipantGraphId);
                throw new StartStreamExtractionException($"Participant {streamExtraction.ParticipantGraphId} is not sharing screen");
            }

            var vbssMediaStream = vbssParticipant.GetScreenShareStream();
            var msi = uint.Parse(vbssMediaStream.SourceId);

            var mediaStreamSettings = GetMediaStreamSettings(msi, streamExtraction);

            var videoSocket = _mediaSocketPool.GetScreenShareSocket();
            _screenShareMediaSocket = _mediaHandlerFactory.CreateSwitchingExtractor(videoSocket, _mediaSocketPool, _mediaSocketPool.MainAudioSocket);

            _screenShareMediaSocket.Start(mediaStreamSettings);

            _isCapturingScreenShare = true;

            StartStreamExtractionResponse response = GetStreamExtractionResponse(mediaStreamSettings.ProtocolSettings);

            return response;
        }

        private void StopScreenShareStreamExtraction(StopStreamExtractionBody streamExtraction)
        {
            lock (_subscriptionLock)
            {
                _screenShareMediaSocket.Stop();

                RemoveAssignedExtractionPort(streamExtraction.ParticipantGraphId, _screenShareMediaSocket.Protocol);

                _mediaSocketPool.ReleaseSocket(_screenShareMediaSocket.VideoSocket);
                _screenShareMediaSocket = null;
            }

            _isCapturingScreenShare = false;
        }

        private IParticipant GetParticipantFromMSI(uint msi)
        {
            return Call.Participants.SingleOrDefault(x => x.Resource.IsInLobby == false && x.Resource.MediaStreams.Any(y => y.SourceId == msi.ToString()));
        }

        private MediaExtractionSettings GetMediaStreamSettings(uint msi, StartStreamExtractionBody streamBody)
        {
            ProtocolSettings protocolSettings;

            if (_currentAssignedPorts.Count == _numberOfMultiviewSockets)
            {
                _logger.LogWarning("[Call Handler] Maximum number of extractions reached");

                throw new StartStreamExtractionException("Maximum number of extractions reached");
            }

            switch (streamBody.Protocol)
            {
                case Protocol.SRT:
                    var port = AssignExtractionPort(streamBody);
                    var srtStreamBody = (SrtStreamExtractionBody)streamBody;

                    protocolSettings = new SrtSettings
                    {
                        Latency = srtStreamBody.Latency,
                        Mode = srtStreamBody.Mode,
                        Passphrase = srtStreamBody.StreamKey,
                        Port = port,
                        Url = srtStreamBody.StreamUrl,
                        AudioFormat = srtStreamBody.AudioFormat,
                        TimeOverlay = srtStreamBody.TimeOverlay,
                        KeyLength = srtStreamBody.KeyLength,
                    };

                    break;
                case Protocol.RTMP:
                    var rtmpPort = AssignExtractionPort(streamBody);
                    var rtmpStreamBody = (RtmpStreamExtractionBody)streamBody;

                    protocolSettings = new RtmpSettings
                    {
                        Mode = rtmpStreamBody.Mode,
                        Port = rtmpPort,
                        EnableSsl = rtmpStreamBody.EnableSsl,
                        StreamKey = rtmpStreamBody.StreamKey,
                        StreamUrl = rtmpStreamBody.StreamUrl,
                        AudioFormat = streamBody.AudioFormat,
                        TimeOverlay = streamBody.TimeOverlay,
                    };
                    break;
                default:
                    throw new ArgumentException("Protocol not supported", nameof(streamBody));
            }

            var mediaStreamSettings = new MediaExtractionSettings
            {
                MediaSourceId = msi,
                MediaType = MediaType.Video,
                VideoResolution = VideoResolution.HD1080p,
                ProtocolSettings = protocolSettings,
            };

            return mediaStreamSettings;
        }

        private void StartSlateInjection()
        {
            var injectionSettings = new MediaInjectionSettings
            {
                CallId = Call.Id,
                StreamId = Call.Id,
            };
            var videoSocket = _mediaSocketPool.GetInjectionVideoSocket();

            _mediaSlateInjector = _mediaHandlerFactory.CreateSlateInjector(videoSocket);
            _mediaSlateInjector.Start(injectionSettings);
        }

        private void StopSlateInjection()
        {
            if (_mediaSlateInjector != null)
            {
                _mediaSlateInjector.Stop();
                _mediaSocketPool.ReleaseSocket(_mediaSlateInjector.VideoSocket);
                _mediaSlateInjector = null;
            }
        }
        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Event fired when the call has been updated.
        /// </summary>
        /// <param name="sender">The call.</param>
        /// <param name="e">The event args containing call changes.</param>
        private async void CallOnUpdated(ICall sender, ResourceEventArgs<Call> e)
        {
            _logger.LogInformation("[CallHandler] On updated call - Old State {oldState} - New State {newState}", e.OldResource.State, e.NewResource.State);

            if (e.OldResource.State != e.NewResource.State && e.NewResource.State == Microsoft.Graph.CallState.Established)
            {
                _logger.LogInformation("[CallHandler] Establishing call {callId}", Call.ScenarioId.ToString());

                var callId = Call.ScenarioId.ToString();
                await _mediatorService.SetCallAsEstablishedAsync(callId, Call.Id);

                _logger.LogInformation("[CallHandler] Call Established {callId}", Call.ScenarioId.ToString());

                // Start injecting a slate image to prevent video rendering issues in MS Teams client related to media bots
                StartSlateInjection();
            }

            if (e.OldResource.State != e.NewResource.State && e.NewResource.State == Microsoft.Graph.CallState.Terminated)
            {
                var callId = Call.ScenarioId.ToString();
                await _mediatorService.SetCallAsTerminatedAsync(callId);
                await _mediatorService.SetBotServiceAsAvailableAsync(callId);
            }
        }

        /// <summary>
        /// Event fired when the participants collection has been updated.
        /// </summary>
        /// <param name="sender">Participants collection.</param>
        /// <param name="args">Event args containing added and removed participants.</param>
        private async void ParticipantsOnUpdated(IParticipantCollection sender, CollectionEventArgs<IParticipant> args)
        {
            foreach (var participant in args.AddedResources)
            {
                // pending remove the cast with the new graph implementation,
                // for now we want the bot to only subscribe to "real" participants
                try
                {
                    _logger.LogInformation("[CallHandler] Adding participant with graph id {id}", participant.Id);
                    _logger.LogDebug("[CallHandler] Adding participant with graph id {id} - Participant: {participant}.", participant.Id, participant.ToJson());
                    if (participant.IsAnAllowedParticipant())
                    {
                        var callId = Call.ScenarioId.ToString();
                        _logger.LogInformation("[CallHandler] Participant with graph id {id} to be added", participant.Id);
                        _logger.LogDebug("[CallHandler] Participant to be added: {participant}", participant.ToJson());

                        await _mediatorService.AddParticipantStreamAsync(callId, participant);

                        // subscribe to the participant updates, this will indicate if the user started to share,
                        // or added another modality
                        participant.OnUpdated += OnParticipantUpdated;
                        _logger.LogInformation("[CallHandler] Participant with graph id {id} has been added.", participant.Id);
                    }
                    else
                    {
                        _logger.LogInformation("[CallHandler] Participant with graph id {id} is not an user/guest/live-bot.", participant.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[CallHandler] Error while trying to add participant with graph id {id}.\nDetails:\n{participant}", participant.Id, participant.ToJson());
                }
            }

            foreach (var participant in args.RemovedResources)
            {
                try
                {
                    // TODO: Support live bots leaving the call
                    _logger.LogInformation("[CallHandler] Removing participant with graph id {id}.", participant.Id);
                    if (participant.IsUser() || participant.IsGuestUser())
                    {
                        // unsubscribe to the participant updates
                        participant.OnUpdated -= OnParticipantUpdated;

                        var callId = Call.ScenarioId.ToString();
                        await _mediatorService.HandleParticipantLeaveAsync(callId, participant.Resource.Id);
                        _logger.LogInformation("[CallHandler] Participant with graph id {id} has been removed.", participant.Id);
                    }
                    else
                    {
                        _logger.LogInformation("[CallHandler] Participant with graph id {id} is not an user/guest.", participant.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[CallHandler] Error while trying to remove participant with graph id {id}.", participant.Id);
                }
            }
        }

        /// <summary>
        /// Event fired when a participant is updated.
        /// </summary>
        /// <param name="sender">Participant object.</param>
        /// <param name="args">Event args containing the old values and the new values.</param>
        private async void OnParticipantUpdated(IParticipant sender, ResourceEventArgs<Participant> args)
        {
            var callId = Call.ScenarioId.ToString();
            try
            {
                _logger.LogInformation("[CallHandler] OnParticipantUpdated - Call {callId} - Participant Graph Id {participantId}", callId, sender.Id);

                // TODO: Analyze what to do with the response
                await _mediatorService.UpdateParticipantMeetingStatusAsync(callId, sender);

                var isParticipantSharingScreen = sender.IsParticipantSharingScreen();
                if (isParticipantSharingScreen && _isCapturingScreenShare)
                {
                    var vbssMediaStream = sender.GetScreenShareStream();
                    _screenShareMediaSocket.SwitchMediaSourceForcefully(uint.Parse(vbssMediaStream.SourceId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CallHandler] Error OnParticipantUpdated - Call {callId} - Participant Graph Id {participantId}", callId, sender.Id);
            }
        }

        /// <summary>
        /// Listen for primary (a.k.a. dominant) speaker changes in the conference.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The dominant speaker changed event arguments.
        /// </param>
        private void OnDominantSpeakerChanged(object sender, DominantSpeakerChangedEventArgs e)
        {
            var callId = Call.ScenarioId.ToString();
            try
            {
                var participant = GetParticipantFromMSI(e.CurrentDominantSpeaker);
                if (participant != null && (participant.IsUser() || participant.IsGuestUser()) && _currentPrimarySpeaker != e.CurrentDominantSpeaker)
                {
                    _currentPrimarySpeaker = e.CurrentDominantSpeaker;

                    if (_isCapturingPrimarySpeaker && _currentPrimarySpeaker != PrimarySpeakerNone)
                    {
                        var participantSendCapableVideoStream = participant.GetParticipantStream();

                        if (participantSendCapableVideoStream != null)
                        {
                            var msi = uint.Parse(participantSendCapableVideoStream.SourceId);
                            if (_currentMediaExtractors.TryGetValue(_primarySpeakerId, out IMediaExtractor mediaSocket))
                            {
                                if (mediaSocket is SwitchingMediaExtractor switchingMediaExtractor)
                                {
                                    switchingMediaExtractor.SwitchMediaSourceSafely(msi);
                                }
                            }
                            else
                            {
                                // TODO: throw a custom exception
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CallHandler] Error OnDominantSpeakerChanged - Call {callId}", callId);
            }
        }
        #endregion
    }
}
