// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Domain.Entities;
using Domain.Entities.Parts;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using static Domain.Constants.Constants;

namespace Application.Stream.Commands
{
    public class RequestStartExtraction
    {
        public class RequestStartExtractionCommand : IRequest<RequestStartExtractionCommandResponse>
        {
            public StartStreamExtractionBody Body { get; set; }
        }

        public class RequestStartExtractionCommandResponse
        {
            public string Id { get; set; }

            public ParticipantStreamModel Resource { get; set; }
        }

        public class RequestStartExtractionCommandValidator : AbstractValidator<RequestStartExtractionCommand>
        {
            public RequestStartExtractionCommandValidator()
            {
                RuleFor(x => x.Body.CallId)
                    .NotEmpty();
                RuleFor(x => x.Body.ResourceType)
                    .IsInEnum();
                RuleFor(x => x.Body.ParticipantId)
                    .NotEmpty();
                RuleFor(x => x.Body.ParticipantGraphId)
                    .NotEmpty();
                RuleFor(x => x.Body.Protocol)
                    .IsInEnum();
            }
        }

        public class RequestStartExtractionCommandHandler : IRequestHandler<RequestStartExtractionCommand, RequestStartExtractionCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IParticipantStreamRepository _participantStreamRepository;

            public RequestStartExtractionCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository,
                IParticipantStreamRepository participantStreamRepository)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
                _participantStreamRepository = participantStreamRepository ?? throw new ArgumentNullException(nameof(participantStreamRepository));
            }

            public async Task<RequestStartExtractionCommandResponse> Handle(RequestStartExtractionCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.Body.CallId);
                request.Body.StreamKey = GetStreamKeyByProtocol(request.Body, call.PrivateContext);

                RequestStartExtractionCommandResponse response = new RequestStartExtractionCommandResponse();

                var command = new DoStartExtraction.DoStartExtractionCommand
                {
                    Body = request.Body,
                };

                var participant = await _participantStreamRepository.GetItemAsync(request.Body.ParticipantId);
                if (participant == null)
                {
                    throw new EntityNotFoundException(nameof(ParticipantStream), request.Body.ParticipantId);
                }

                var service = await _serviceRepository.GetItemAsync(call.ServiceId);

                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);

                participant.State = StreamState.Starting;
                participant.Details.TimeOverlay = request.Body.TimeOverlay;
                participant.Details.AudioFormat = request.Body.AudioFormat;

                await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                try
                {
                    var botServiceResponse = await _botServiceClient.StartExtractionAsync(command);

                    response.Id = participant.Id;
                    response.Resource = botServiceResponse.Resource;

                    return response;
                }
                catch (Exception)
                {
                    participant.State = StreamState.Disconnected;
                    participant.Error = new StreamErrorDetails(StreamErrorType.StartExtraction, Messages.StartExtraction.Error);

                    await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                    throw;
                }
            }

            private static string GetStreamKeyFromPrivateCallContext(Dictionary<string, string> privateCallContext)
            {
                if (!privateCallContext.TryGetValue("streamKey", out string streamKey))
                {
                    throw new StartStreamExtractionException("Stream key is not configured for this call, RTMP Extraction in pull mode could not be initiated");
                }

                return streamKey;
            }

            private static string GetStreamKeyByProtocol(StartStreamExtractionBody startStreamExtractionBody, Dictionary<string, string> privateCallContext)
            {
                if (startStreamExtractionBody.Protocol == Protocol.RTMP)
                {
                    var rtmpStartStreamExtractionBody = startStreamExtractionBody as RtmpStreamExtractionBody;

                    return rtmpStartStreamExtractionBody.Mode == RtmpMode.Pull ?
                        GetStreamKeyFromPrivateCallContext(privateCallContext) :
                        rtmpStartStreamExtractionBody.StreamKey;
                }

                return startStreamExtractionBody.StreamKey;
            }
        }
    }
}
