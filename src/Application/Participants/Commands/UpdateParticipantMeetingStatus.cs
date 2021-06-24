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
    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>
        ///     Command Response
        /// </summary>
        public class UpdateParticipantMeetingStatusCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        //Should add validator???

        /// <summary>
        /// /
        /// </summary>
        public class UpdateParticipantMeetingStatusCommandHandler : IRequestHandler<UpdateParticipantMeetingStatusCommand, UpdateParticipantMeetingStatusCommandResponse>
        {
            private readonly IParticipantStreamRepository participantStreamRepository;
            private readonly IMapper mapper;
            private readonly ILogger<UpdateParticipantMeetingStatusCommandHandler> logger;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="participantStreamRepository"></param>
            /// <param name="mapper"></param>
            /// <param name="logger"></param>
            public UpdateParticipantMeetingStatusCommandHandler(IParticipantStreamRepository participantStreamRepository,
                IMapper mapper,
                ILogger<UpdateParticipantMeetingStatusCommandHandler> logger)
            {
                this.participantStreamRepository = participantStreamRepository;
                this.mapper = mapper;
                this.logger = logger;
            }

            public async Task<UpdateParticipantMeetingStatusCommandResponse> Handle(UpdateParticipantMeetingStatusCommand command, CancellationToken cancellationToken)
            {
                UpdateParticipantMeetingStatusCommandResponse response = new UpdateParticipantMeetingStatusCommandResponse();
                var specification = new ParticipantStreamGetFromCallSpecification(command.CallId, command.ParticipantGraphId);

                //TODO: Analyze if we should change our cosmos db repository
                var participants = await this.participantStreamRepository.GetItemsAsync(specification);
                
                var participant = participants.FirstOrDefault();
                if (participant == null)
                {
                    logger.LogInformation("Participant {participantId} from call {callId} was not found", command.ParticipantGraphId, command.CallId);
                    throw new EntityNotFoundException($"Participant {command.ParticipantGraphId} from call {command.CallId} wasn't found");
                }

                mapper.Map<UpdateParticipantMeetingStatusCommand, ParticipantStream>(command, participant);

                await this.participantStreamRepository.UpdateItemAsync(participant.Id, participant);

                return response;
            }
        }
    }
}
