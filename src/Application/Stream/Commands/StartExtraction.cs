using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Ardalis.Result;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Stream.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class StartExtraction
    {
        /// <summary>
        /// 
        /// </summary>
        public class StartExtractionCommand : IRequest<StartExtractionCommandResponse>
        {
            public StartStreamExtractionBody Body { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class StartExtractionCommandResponse
        {
            public string Id { get; set; }
            public ParticipantStreamModel Resource { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class StartExtractionCommandValidator : AbstractValidator<StartExtractionCommand>
        {
            public StartExtractionCommandValidator()
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

        /// <summary>
        /// 
        /// </summary>
        public class StartExtractionCommandHandler : IRequestHandler<StartExtractionCommand, StartExtractionCommandResponse>
        {
            private readonly IBot bot;
            private readonly IParticipantStreamRepository participantStreamRepository;
            private readonly IMapper mapper;

            public StartExtractionCommandHandler(
                IBot bot,
                IParticipantStreamRepository participantStreamRepository,
                IMapper mapper
                )
            {
                this.bot = bot ?? throw new ArgumentNullException(nameof(bot));
                this.participantStreamRepository = participantStreamRepository ?? throw new ArgumentNullException(nameof(participantStreamRepository));
                this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<StartExtractionCommandResponse> Handle(StartExtractionCommand request, CancellationToken cancellationToken)
            {
                //TODO: Change parameter, participant Id is not the key. It is the Id assigned by the teams call
                var participant = await participantStreamRepository.GetItemAsync(request.Body.ParticipantId);

                if (participant == null)
                {
                    throw new EntityNotFoundException(nameof(ParticipantStream), request.Body.ParticipantId);
                }

                try
                {
                    StartStreamExtractionResponse startExtractionResponse = bot.StartExtraction(request.Body);

                    participant.State = StreamState.Started;
                    participant.Details.TimeOverlay = startExtractionResponse.TimeOverlay;
                    participant.Details.AudioFormat = startExtractionResponse.AudioFormat;

                    switch (startExtractionResponse.Protocol)
                    {
                        case Protocol.SRT:
                            var srtExtractionResponse = (StartSrtStreamExtractionResponse)startExtractionResponse;

                            participant.Details.Latency = srtExtractionResponse.Latency;
                            participant.Details.StreamKey = srtExtractionResponse.Passphrase;
                            participant.Details.StreamUrl = srtExtractionResponse.Url;

                            break;
                        case Protocol.RTMP:
                            var rtmpExtractionResponse = (StartRtmpStreamExtractionResponse)startExtractionResponse;

                            participant.Details.StreamUrl = rtmpExtractionResponse.StreamUrl;
                            participant.Details.StreamKey = rtmpExtractionResponse.StreamKey;

                            break;
                        default:
                            break;
                    }

                    await participantStreamRepository.UpdateItemAsync(participant.Id, participant);
                }
                catch (Exception ex)
                {
                    participant.State = StreamState.Disconnected;
                    participant.Error = new StreamErrorDetails(StreamErrorType.StartExtraction, ex.Message);

                    await participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                    throw;
                }

                StartExtractionCommandResponse response = new StartExtractionCommandResponse
                {
                    Id = participant.Id,
                    Resource = mapper.Map<ParticipantStreamModel>(participant)
                };

                return response;
            }
        }
    }
}
