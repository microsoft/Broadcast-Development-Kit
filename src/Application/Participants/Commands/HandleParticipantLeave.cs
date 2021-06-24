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
        /// <summary>
        /// 
        /// </summary>
        public class HandleParticipantLeaveCommand : IRequest<HandleParticipantLeaveCommandResponse>
        {
            public string CallId { get; set; }

            public string ParticipantId { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class HandleParticipantLeaveCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        //TODO: Analyze if validator is necessary

        /// <summary>
        /// 
        /// </summary>
        public class HandleParticipantLeaveCommandHandler : IRequestHandler<HandleParticipantLeaveCommand, HandleParticipantLeaveCommandResponse>
        {
            private readonly IParticipantStreamRepository participantStreamRepository;
            private readonly ILogger<HandleParticipantLeaveCommandHandler> logger;

            /// <summary>
            ///     Ctor
            /// </summary>
            /// <param name="participantStreamRepository"></param>
            /// <param name="logger"></param>
            public HandleParticipantLeaveCommandHandler(IParticipantStreamRepository participantStreamRepository,
                ILogger<HandleParticipantLeaveCommandHandler> logger)
            {
                this.participantStreamRepository = participantStreamRepository;
                this.logger = logger;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="request"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<HandleParticipantLeaveCommandResponse> Handle(HandleParticipantLeaveCommand request, CancellationToken cancellationToken)
            {
                HandleParticipantLeaveCommandResponse response = new HandleParticipantLeaveCommandResponse();
                var specification = new ParticipantStreamGetFromCallSpecification(request.CallId, request.ParticipantId);

                //TODO: Analyze if we should change our cosmos db repository
                var participants = await participantStreamRepository.GetItemsAsync(specification);

                var participant = participants.FirstOrDefault();
                if (participant == null)
                {
                    logger.LogInformation("Participant {participantId} from call {callId} was not found", request.ParticipantId, request.CallId);
                    throw new EntityNotFoundException($"Participant {request.ParticipantId} from call {request.CallId} wasn't found");
                }

                participant.LeftAt = DateTime.Now;

                await this.participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                return response;
            }
        }
    }
}
