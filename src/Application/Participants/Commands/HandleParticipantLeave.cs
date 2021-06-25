using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Application.Participants.Specifications;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Participant.Commands
{
    public class HandleParticipantLeave
    {
        public class HandleParticipantLeaveCommand : IRequest<HandleParticipantLeaveCommandResponse>
        {
            public string CallId { get; set; }

            public string ParticipantId { get; set; }
        }

        public class HandleParticipantLeaveCommandResponse
        {
            public string Id { get; set; }
        }

        // TODO: Analyze if validator is necessary
        public class HandleParticipantLeaveCommandHandler : IRequestHandler<HandleParticipantLeaveCommand, HandleParticipantLeaveCommandResponse>
        {
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly ILogger<HandleParticipantLeaveCommandHandler> _logger;

            public HandleParticipantLeaveCommandHandler(
                IParticipantStreamRepository participantStreamRepository,
                ILogger<HandleParticipantLeaveCommandHandler> logger)
            {
                _participantStreamRepository = participantStreamRepository;
                _logger = logger;
            }

            public async Task<HandleParticipantLeaveCommandResponse> Handle(HandleParticipantLeaveCommand request, CancellationToken cancellationToken)
            {
                HandleParticipantLeaveCommandResponse response = new HandleParticipantLeaveCommandResponse();
                var specification = new ParticipantStreamGetFromCallSpecification(request.CallId, request.ParticipantId);

                // TODO: Analyze if we should change our cosmos db repository
                var participants = await _participantStreamRepository.GetItemsAsync(specification);

                var participant = participants.FirstOrDefault();
                if (participant == null)
                {
                    _logger.LogInformation("Participant {participantId} from call {callId} was not found", request.ParticipantId, request.CallId);
                    throw new EntityNotFoundException($"Participant {request.ParticipantId} from call {request.CallId} wasn't found");
                }

                participant.LeftAt = DateTime.Now;

                await _participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                return response;
            }
        }
    }
}
