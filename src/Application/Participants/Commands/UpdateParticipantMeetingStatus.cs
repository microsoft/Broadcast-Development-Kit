// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Application.Participants.Specifications;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Participant.Commands
{
    public class UpdateParticipantMeetingStatus
    {
        public class UpdateParticipantMeetingStatusCommand : IRequest<UpdateParticipantMeetingStatusCommandResponse>
        {
            public string CallId { get; set; }

            public string ParticipantGraphId { get; set; }

            public bool AudioMuted { get; set; }

            public bool IsSharingAudio { get; set; }

            public bool IsSharingVideo { get; set; }

            public bool IsSharingScreen { get; set; }
        }

        public class UpdateParticipantMeetingStatusCommandResponse
        {
            public string Id { get; set; }
        }

        // Should add validator???
        public class UpdateParticipantMeetingStatusCommandHandler : IRequestHandler<UpdateParticipantMeetingStatusCommand, UpdateParticipantMeetingStatusCommandResponse>
        {
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly IMapper _mapper;
            private readonly ILogger<UpdateParticipantMeetingStatusCommandHandler> _logger;

            public UpdateParticipantMeetingStatusCommandHandler(
                IParticipantStreamRepository participantStreamRepository,
                IMapper mapper,
                ILogger<UpdateParticipantMeetingStatusCommandHandler> logger)
            {
                _participantStreamRepository = participantStreamRepository;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<UpdateParticipantMeetingStatusCommandResponse> Handle(UpdateParticipantMeetingStatusCommand command, CancellationToken cancellationToken)
            {
                UpdateParticipantMeetingStatusCommandResponse response = new UpdateParticipantMeetingStatusCommandResponse();
                var specification = new ParticipantStreamGetFromCallSpecification(command.CallId, command.ParticipantGraphId);

                // TODO: Analyze if we should change our cosmos db repository
                var participants = await _participantStreamRepository.GetItemsAsync(specification);

                var participant = participants.FirstOrDefault();
                if (participant == null)
                {
                    _logger.LogInformation("Participant {participantId} from call {callId} was not found", command.ParticipantGraphId, command.CallId);
                    throw new EntityNotFoundException($"Participant {command.ParticipantGraphId} from call {command.CallId} wasn't found");
                }

                _mapper.Map<UpdateParticipantMeetingStatusCommand, ParticipantStream>(command, participant);

                await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                return response;
            }
        }
    }
}
