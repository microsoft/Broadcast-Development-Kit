using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using static Domain.Constants.Constants;

namespace Application.Stream.Commands
{
    public class StoppingExtraction
    {
        public class StoppingExtractionCommand : IRequest<StoppingExtractionCommandResponse>
        {
            public StopStreamExtractionBody Body { get; set; }
        }

        public class StoppingExtractionCommandResponse
        {
            public string Id { get; set; }

            public ParticipantStreamModel Resource { get; set; }
        }

        public class StoppingExtractionCommandValidator : AbstractValidator<StoppingExtractionCommand>
        {
            public StoppingExtractionCommandValidator()
            {
                RuleFor(x => x.Body.CallId)
                    .NotEmpty();
                RuleFor(x => x.Body.ResourceType)
                    .IsInEnum();
                RuleFor(x => x.Body.ParticipantId)
                    .NotEmpty();
                RuleFor(x => x.Body.ParticipantGraphId)
                    .NotEmpty();
            }
        }

        public class StoppingExtractionCommandHandler : IRequestHandler<StoppingExtractionCommand, StoppingExtractionCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IParticipantStreamRepository _participantStreamRepository;

            public StoppingExtractionCommandHandler(
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

            public async Task<StoppingExtractionCommandResponse> Handle(StoppingExtractionCommand request, CancellationToken cancellationToken)
            {
                StoppingExtractionCommandResponse response = new StoppingExtractionCommandResponse();

                var command = new StopExtraction.StopExtractionCommand
                {
                    Body = request.Body,
                };

                var participant = await _participantStreamRepository.GetItemAsync(request.Body.ParticipantId);
                if (participant == null)
                {
                    throw new EntityNotFoundException(nameof(ParticipantStream), request.Body.ParticipantId);
                }

                var call = await _callRepository.GetItemAsync(request.Body.CallId);
                var service = await _serviceRepository.GetItemAsync(call.ServiceId);

                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);

                participant.State = StreamState.Stopping;

                await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                try
                {
                    var botServiceResponse = await _botServiceClient.StopExtractionAsync(command);
                    response.Id = participant.Id;
                    response.Resource = botServiceResponse.Resource;

                    return response;
                }
                catch (Exception)
                {
                    participant.State = StreamState.Disconnected;
                    participant.Error = new StreamErrorDetails(StreamErrorType.StartExtraction, Messages.StopExtraction.Error);

                    await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                    throw;
                }
            }
        }
    }
}
