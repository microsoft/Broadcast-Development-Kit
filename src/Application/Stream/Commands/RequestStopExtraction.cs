// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
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
    public class RequestStopExtraction
    {
        public class RequestStopExtractionCommand : IRequest<RequestStopExtractionCommandResponse>
        {
            public StopStreamExtractionBody Body { get; set; }
        }

        public class RequestStopExtractionCommandResponse
        {
            public string Id { get; set; }

            public ParticipantStreamModel Resource { get; set; }
        }

        public class RequestStopExtractionCommandValidator : AbstractValidator<RequestStopExtractionCommand>
        {
            public RequestStopExtractionCommandValidator()
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

        public class RequestStopExtractionCommandHandler : IRequestHandler<RequestStopExtractionCommand, RequestStopExtractionCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IParticipantStreamRepository _participantStreamRepository;

            public RequestStopExtractionCommandHandler(
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

            public async Task<RequestStopExtractionCommandResponse> Handle(RequestStopExtractionCommand request, CancellationToken cancellationToken)
            {
                RequestStopExtractionCommandResponse response = new RequestStopExtractionCommandResponse();

                var command = new DoStopExtraction.DoStopExtractionCommand
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
