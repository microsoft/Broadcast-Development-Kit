using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Stream.Commands
{
    public class StartExtraction
    {
        public class StartExtractionCommand : IRequest<StartExtractionCommandResponse>
        {
            public StartStreamExtractionBody Body { get; set; }
        }

        public class StartExtractionCommandResponse
        {
            public string Id { get; set; }

            public ParticipantStreamModel Resource { get; set; }
        }

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

        public class StartExtractionCommandHandler : IRequestHandler<StartExtractionCommand, StartExtractionCommandResponse>
        {
            private readonly IBot _bot;
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly IMapper _mapper;

            public StartExtractionCommandHandler(
                IBot bot,
                IParticipantStreamRepository participantStreamRepository,
                IMapper mapper)
            {
                _bot = bot ?? throw new ArgumentNullException(nameof(bot));
                _participantStreamRepository = participantStreamRepository ?? throw new ArgumentNullException(nameof(participantStreamRepository));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<StartExtractionCommandResponse> Handle(StartExtractionCommand request, CancellationToken cancellationToken)
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

                    await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);
                }
                catch (Exception ex)
                {
                    participant.State = StreamState.Disconnected;
                    participant.Error = new StreamErrorDetails(StreamErrorType.StartExtraction, ex.Message);

                    await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                    throw;
                }

                StartExtractionCommandResponse response = new StartExtractionCommandResponse
                {
                    Id = participant.Id,
                    Resource = _mapper.Map<ParticipantStreamModel>(participant),
                };

                return response;
            }
        }
    }
}
