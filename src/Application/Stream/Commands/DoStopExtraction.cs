// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
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
    public class DoStopExtraction
    {
        public class DoStopExtractionCommand : IRequest<DoStopExtractionCommandResponse>
        {
            public StopStreamExtractionBody Body { get; set; }
        }

        public class DoStopExtractionCommandResponse
        {
            public string Id { get; set; }

            public ParticipantStreamModel Resource { get; set; }
        }

        public class DoStopExtractionCommandValidator : AbstractValidator<DoStopExtractionCommand>
        {
            public DoStopExtractionCommandValidator()
            {
                RuleFor(x => x.Body.CallId)
                   .NotEmpty();
                RuleFor(x => x.Body.ParticipantId)
                  .NotEmpty();
                RuleFor(x => x.Body.ResourceType)
                    .IsInEnum();
                RuleFor(x => x.Body.ParticipantGraphId)
                   .NotEmpty();
            }
        }

        public class DoStopExtractionCommandHandler : IRequestHandler<DoStopExtractionCommand, DoStopExtractionCommandResponse>
        {
            private readonly IBot _bot;
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly IMapper _mapper;

            public DoStopExtractionCommandHandler(
                IBot bot,
                IParticipantStreamRepository participantStreamRepository,
                IMapper mapper)
            {
                _bot = bot ?? throw new ArgumentNullException(nameof(bot));
                _participantStreamRepository = participantStreamRepository ?? throw new ArgumentNullException(nameof(participantStreamRepository));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<DoStopExtractionCommandResponse> Handle(DoStopExtractionCommand request, CancellationToken cancellationToken)
            {
                // TODO: Change parameter, participant Id is not the key. It is the Id assigned by the teams call
                var participant = await _participantStreamRepository.GetItemAsync(request.Body.ParticipantId);

                if (participant == null)
                {
                    throw new EntityNotFoundException(nameof(ParticipantStream), request.Body.ParticipantId);
                }

                try
                {
                    _bot.StopExtraction(request.Body);

                    participant.State = StreamState.Disconnected;
                    participant.Details = new ParticipantStreamDetails();
                    participant.Error = null;

                    await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);
                }
                catch (Exception ex)
                {
                    participant.State = StreamState.Disconnected;
                    participant.Error = new StreamErrorDetails(StreamErrorType.StartExtraction, ex.Message);

                    await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                    throw;
                }

                DoStopExtractionCommandResponse response = new DoStopExtractionCommandResponse
                {
                    Id = participant.Id,
                    Resource = _mapper.Map<ParticipantStreamModel>(participant),
                };

                return response;
            }
        }
    }
}
