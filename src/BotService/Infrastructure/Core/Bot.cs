// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Config;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Service.Commands;
using BotService.Application.Core;
using BotService.Infrastructure.Common;
using BotService.Infrastructure.Extensions;
using BotService.Infrastructure.Pipelines;
using BotService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Calls;
using Microsoft.Graph.Communications.Calls.Media;
using Microsoft.Graph.Communications.Client;
using Microsoft.Graph.Communications.Common;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Resources;
using Microsoft.Skype.Bots.Media;

namespace BotService.Infrastructure.Core
{
    public class Bot : IBot
    {
        private readonly ICommunicationsClient _client;
        private readonly IMediatorService _mediatorService;
        private readonly IMediaHandlerFactory _mediaHandlerFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly BotConfiguration _config;
        private readonly ILogger<Bot> _logger;
        private readonly GstreamerClockProvider _clockProvider;

        public Bot(
            ICommunicationsClient client,
            IMediatorService mediatorService,
            IMediaHandlerFactory mediaHandlerFactory,
            IAppConfiguration config,
            GstreamerClockProvider clockProvider,
            ILoggerFactory loggerFactory)
        {
            _client = client;
            _mediatorService = mediatorService;
            _mediaHandlerFactory = mediaHandlerFactory;
            _clockProvider = clockProvider;
            _loggerFactory = loggerFactory;

            _config = config.BotConfiguration;
            _logger = loggerFactory.CreateLogger<Bot>();

            _client.Calls().OnIncoming += CallsOnIncoming;
            _client.Calls().OnUpdated += CallsOnUpdated;
        }

        /// <summary>
        /// Gets the collection of call handlers.
        /// </summary>
        public ConcurrentDictionary<string, CallHandler> CallHandlers { get; } = new ConcurrentDictionary<string, CallHandler>();

        public string Id { get; private set; }

        public string VirtualMachineName { get; set; }

        public async Task InviteBotAsync(InviteBot.InviteBotCommand command)
        {
            _logger.LogInformation("[Bot] Getting meeting info for call {callId}", command.CallId);

            MeetingInfo meetingInfo;
            ChatInfo chatInfo;

            (chatInfo, meetingInfo) = JoinInfoHelper.ParseJoinURL(command.MeetingUrl);

            var tenantId = (meetingInfo as OrganizerMeetingInfo)?.Organizer.GetPrimaryIdentity()?.GetTenantId();
            var mediaSession = CreateLocalMediaSession();

            var joinParams = new JoinMeetingParameters(chatInfo, meetingInfo, mediaSession)
            {
                TenantId = tenantId,
            };

            var scenarioId = Guid.Parse(command.CallId);

            _logger.LogInformation("[Bot] Initiating call {callId} with scenario id {scenarioId}", command.CallId, scenarioId);

            var statefulCall = await _client.Calls().AddAsync(joinParams, scenarioId).ConfigureAwait(false);

            _logger.LogInformation("[Bot] Call initialization completed - call {callId}", command.CallId);

            statefulCall.GraphLogger.Info($"Call creation complete: {statefulCall.Id}");

            // TODO: Analyze if we need to return something
        }

        public async Task ProcessNotificationAsync(HttpRequestMessage request)
        {
            await _client.ProcessNotificationAsync(request).ConfigureAwait(false);
        }

        public async Task RegisterServiceAsync(string virtualMachineName)
        {
            var response = await _mediatorService.RegisterServiceAsync(virtualMachineName);
            Id = response.Id;
        }

        public async Task UnregisterServiceAsync(string virtualMachineName)
        {
            var response = await _mediatorService.UnregisterServiceAsync(virtualMachineName);
            Id = response.Id;
        }

        public async Task MuteBotAsync()
        {
            var callHandler = CallHandlers.First().Value;
            await callHandler.Call.MuteAsync();
        }

        public async Task UnmuteBotAsync()
        {
            var callHandler = CallHandlers.First().Value;
            await callHandler.Call.UnmuteAsync();
        }

        public async Task RemoveBotAsync(string callGraphId)
        {
            try
            {
                var callHandler = GetHandlerOrThrow(callGraphId);
                callHandler.StopActiveStreams();
                await callHandler.Call.DeleteAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Manually remove the call from SDK state.
                // This will trigger the ICallCollection.OnUpdated event with the removed resource.
                _client.Calls().TryForceRemove(callGraphId, out _);
            }
        }

        // TODO: Define Output
        public void StartInjection(StartStreamInjectionBody startStreamInjectionBody)
        {
            var callHandler = CallHandlers.First().Value;
            callHandler.StartInjection(startStreamInjectionBody);
        }

        public void StopInjection()
        {
            var callHandler = CallHandlers.First().Value;
            callHandler.StopInjection();
        }

        public StartStreamExtractionResponse StartExtraction(StartStreamExtractionBody streamBody)
        {
            var callHandler = CallHandlers.First().Value;
            return callHandler.StartExtraction(streamBody);
        }

        public void StopExtraction(StopStreamExtractionBody streamBody)
        {
            var callHandler = CallHandlers.First().Value;
            callHandler.StopExtraction(streamBody);
        }

        #region Private

        /// <summary>
        /// Creates the local media session.
        /// </summary>
        /// <param name="mediaSessionId">
        /// The media session identifier.
        /// This should be a unique value for each call.
        /// </param>
        /// <returns>The <see cref="ILocalMediaSession"/>.</returns>
        private ILocalMediaSession CreateLocalMediaSession(Guid mediaSessionId = default)
        {
            var videoSocketSettings = new List<VideoSocketSettings>
            {
                new VideoSocketSettings
                {
                    StreamDirections = StreamDirection.Sendrecv,
                    ReceiveColorFormat = VideoColorFormat.H264,
                    SupportedSendVideoFormats = new List<VideoFormat>
                    {
                        VideoFormat.NV12_1280x720_30Fps,
                        VideoFormat.NV12_1920x1080_30Fps,
                        VideoFormat.NV12_1920x1080_1_875Fps,
                    },
                    MaxConcurrentSendStreams = 1,
                },
            };

            // create the receive only sockets settings for the multiview support
            for (int i = 0; i < _config.NumberOfMultiviewSockets; i++)
            {
                videoSocketSettings.Add(new VideoSocketSettings
                {
                    StreamDirections = StreamDirection.Recvonly,
                    ReceiveColorFormat = VideoColorFormat.H264,
                });
            }

            // Create the VBSS socket settings
            var vbssSocketSettings = new VideoSocketSettings
            {
                StreamDirections = StreamDirection.Recvonly,
                ReceiveColorFormat = VideoColorFormat.H264,
                MediaType = MediaType.Vbss,
                SupportedSendVideoFormats = new List<VideoFormat>
                {
                    // fps 1.875 is required for h264 in vbss scenario.
                    VideoFormat.H264_1920x1080_1_875Fps,
                },
            };

            // create media session object, this is needed to establish call connections
            var mediaSession = _client.CreateMediaSession(
                new AudioSocketSettings
                {
                    StreamDirections = StreamDirection.Sendrecv,
                    SupportedAudioFormat = AudioFormat.Pcm16K,
                },
                videoSocketSettings,
                vbssSocketSettings,
                mediaSessionId: mediaSessionId);
            return mediaSession;
        }

        /// <summary>
        /// Incoming call handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="CollectionEventArgs{TEntity}"/> instance containing the event data.</param>
        private void CallsOnIncoming(ICallCollection sender, CollectionEventArgs<ICall> args)
        {
            args.AddedResources.ForEach(call =>
            {
                IMediaSession mediaSession = Guid.TryParse(call.Id, out Guid callId)
                    ? CreateLocalMediaSession(callId)
                    : CreateLocalMediaSession();

                // Answer call
                call?.AnswerAsync(mediaSession).ForgetAndLogExceptionAsync(
                    call.GraphLogger,
                    $"Answering call {call.Id} with scenario {call.ScenarioId}.");
            });
        }

        /// <summary>
        /// Updated call handler.
        /// </summary>
        /// <param name="sender">The <see cref="ICallCollection"/> sender.</param>
        /// <param name="args">The <see cref="CollectionEventArgs{ICall}"/> instance containing the event data.</param>
        private void CallsOnUpdated(ICallCollection sender, CollectionEventArgs<ICall> args)
        {
            foreach (var call in args.AddedResources)
            {
                var callHandler = new CallHandler(call, _config, _mediatorService, _mediaHandlerFactory, _loggerFactory, _clockProvider);
                CallHandlers[call.Id] = callHandler;
            }

            foreach (var call in args.RemovedResources)
            {
                if (CallHandlers.TryRemove(call.Id, out CallHandler handler))
                {
                    handler.Dispose();
                }
            }
        }

        /// <summary>
        /// The get handler or throw.
        /// </summary>
        /// <param name="callLegId">
        /// The call leg id.
        /// </param>
        /// <returns>
        /// The <see cref="CallHandler"/>.
        /// </returns>
        /// <exception cref="ObjectNotFoundException">
        /// Throws an exception if handler is not found.
        /// </exception>
        private CallHandler GetHandlerOrThrow(string callLegId)
        {
            if (!CallHandlers.TryGetValue(callLegId, out CallHandler handler))
            {
                throw new ObjectNotFoundException($"call ({callLegId}) not found");
            }

            return handler;
        }
        #endregion
    }
}
