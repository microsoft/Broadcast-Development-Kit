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
    public class StopExtraction
    {
        /// <summary>
        /// 
        /// </summary>
        public class StopExtractionCommand : IRequest<StopExtractionCommandResponse>
        {
            public StopStreamExtractionBody Body { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class StopExtractionCommandResponse
        {
            public string Id { get; set; }
            public ParticipantStreamModel Resource { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class StopExtractionCommandValidator : AbstractValidator<StopExtractionCommand>
        {
            public StopExtractionCommandValidator()
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

        /// <summary>
        /// 
        /// </summary>
        public class StopExtractionCommandHandler : IRequestHandler<StopExtractionCommand, StopExtractionCommandResponse>
        {
            private readonly IBot _bot;
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly IMapper _mapper;

            public StopExtractionCommandHandler(
                IBot bot,
                IParticipantStreamRepository participantStreamRepository,
                IMapper mapper
                )
            {
                _bot = bot ?? throw new ArgumentNullException(nameof(bot));
                _participantStreamRepository = participantStreamRepository ?? throw new ArgumentNullException(nameof(participantStreamRepository));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<StopExtractionCommandResponse> Handle(StopExtractionCommand request, CancellationToken cancellationToken)
            {
                //TODO: Change parameter, participant Id is not the key. It is the Id assigned by the teams call
                var participant = await _participantStreamRepository.GetItemAsync(request.Body.ParticipantId);
                
                if (participant == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.ParticipantStream), request.Body.ParticipantId);
                }

                try
                {
                    _bot.StopExtraction(request.Body);

                    participant.State = StreamState.Disconnected;
                    participant.Details = new Domain.Entities.ParticipantStreamDetails();
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

                StopExtractionCommandResponse response = new StopExtractionCommandResponse
                { 
                    Id = participant.Id,
                    Resource = _mapper.Map<ParticipantStreamModel>(participant)
                };

                return response;
            }
        }
    }
}
