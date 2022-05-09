// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Config;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Parts;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Stream.Commands
{
    public class DoStartExtraction
    {
        public class DoStartExtractionCommand : IRequest<DoStartExtractionCommandResponse>
        {
            public StartStreamExtractionBody Body { get; set; }
        }

        public class DoStartExtractionCommandResponse
        {
            public string Id { get; set; }

            public ParticipantStreamModel Resource { get; set; }
        }

        public class DoStartExtractionCommandValidator : AbstractValidator<DoStartExtractionCommand>
        {
            public DoStartExtractionCommandValidator()
            {
                RuleFor(x => x.Body.CallId)
                   .NotEmpty();
                RuleFor(x => x.Body.ParticipantId)
                  .NotEmpty();
                RuleFor(x => x.Body.ResourceType)
                    .IsInEnum();
                RuleFor(x => x.Body.ParticipantGraphId)
                   .NotEmpty();
                RuleFor(x => x.Body.Protocol)
                    .IsInEnum();
            }
        }

        public class DoStartExtractionCommandHandler : IRequestHandler<DoStartExtractionCommand, DoStartExtractionCommandResponse>
        {
            private readonly IAppConfiguration _configuration;
            private readonly IBot _bot;
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly IMapper _mapper;
            private readonly IExtractionUrlHelper _extractionUrlHelper;

            public DoStartExtractionCommandHandler(
                IAppConfiguration configuration,
                IBot bot,
                IParticipantStreamRepository participantStreamRepository,
                IExtractionUrlHelper extractionUrlHelper,
                IMapper mapper)
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _bot = bot ?? throw new ArgumentNullException(nameof(bot));
                _participantStreamRepository = participantStreamRepository ?? throw new ArgumentNullException(nameof(participantStreamRepository));
                _extractionUrlHelper = extractionUrlHelper ?? throw new ArgumentNullException(nameof(extractionUrlHelper));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<DoStartExtractionCommandResponse> Handle(DoStartExtractionCommand request, CancellationToken cancellationToken)
            {
                // TODO: Change parameter, participant Id is not the key. It is the Id assigned by the teams call
                var participant = await _participantStreamRepository.GetItemAsync(request.Body.ParticipantId);

                if (participant == null)
                {
                    throw new EntityNotFoundException(nameof(ParticipantStream), request.Body.ParticipantId);
                }

                try
                {
                    StartStreamExtractionResponse startExtractionResponse = _bot.StartExtraction(request.Body);

                    participant.State = StreamState.Ready;
                    participant.Details.TimeOverlay = startExtractionResponse.TimeOverlay;
                    participant.Details.AudioFormat = startExtractionResponse.AudioFormat;

                    switch (startExtractionResponse.Protocol)
                    {
                        case Protocol.SRT:
                            var srtExtractionResponse = (StartSrtStreamExtractionResponse)startExtractionResponse;

                            participant.Details.Latency = srtExtractionResponse.Latency;
                            participant.Details.StreamKey = srtExtractionResponse.Passphrase;
                            participant.Details.StreamUrl = _extractionUrlHelper.GetSrtStreamUrl(srtExtractionResponse, _configuration.BotConfiguration.ServiceFqdn);
                            participant.Details.KeyLength = srtExtractionResponse.KeyLength;

                            break;
                        case Protocol.RTMP:
                            var rtmpExtractionResponse = (StartRtmpStreamExtractionResponse)startExtractionResponse;

                            participant.Details.StreamUrl = _extractionUrlHelper.GetRtmpStreamUrl(rtmpExtractionResponse, participant.CallId, _configuration.BotConfiguration.ServiceDnsName);
                            participant.Details.StreamKey = rtmpExtractionResponse.StreamKey;

                            break;
                        default:
                            break;
                    }

                    await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);
                }
                catch (Exception ex)
                {
                    participant.State = StreamState.Disconnected;
                    participant.Error = new StreamErrorDetails(StreamErrorType.StartExtraction, ex.Message);

                    await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                    throw;
                }

                DoStartExtractionCommandResponse response = new DoStartExtractionCommandResponse
                {
                    Id = participant.Id,
                    Resource = _mapper.Map<ParticipantStreamModel>(participant),
                };

                return response;
            }
        }
    }
}
