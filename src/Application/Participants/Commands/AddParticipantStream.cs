using Application.Common.Config;
using Application.Common.Models;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Participant.Commands
{
    public class AddParticipantStream
    {
        public class AddParticipantStreamCommand : IRequest<AddParticipantStreamCommandResponse>
        {
            public ParticipantStreamModel Participant { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class AddParticipantStreamCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        //TODO: Analyze if we should add a validator

        /// <summary>
        /// 
        /// </summary>
        public class AddParticipantStreamCommandHandler : IRequestHandler<AddParticipantStreamCommand, AddParticipantStreamCommandResponse>
        {
            private readonly IParticipantStreamRepository participantStreamRepository;
            private readonly IMapper mapper;
            private readonly IAppConfiguration appConfiguartion;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="participantStreamRepository"></param>
            /// <param name="mapper"></param>
            /// <param name="appConfiguartion"></param>
            /// <param name="logger"></param>
            public AddParticipantStreamCommandHandler(IParticipantStreamRepository participantStreamRepository,
                IMapper mapper,
                IAppConfiguration appConfiguartion
                )
            {
                this.participantStreamRepository = participantStreamRepository;
                this.mapper = mapper;
                this.appConfiguartion = appConfiguartion;
            }

            public async Task<AddParticipantStreamCommandResponse> Handle(AddParticipantStreamCommand request, CancellationToken cancellationToken)
            {
                AddParticipantStreamCommandResponse response = new AddParticipantStreamCommandResponse();

                var entity = mapper.Map<ParticipantStream>(request.Participant);
                entity.PhotoUrl = $"https://{appConfiguartion.BotConfiguration.MainApiUrl}/api/participant/photo/{entity.AadId}";

                await participantStreamRepository.AddItemAsync(entity);

                response.Id = entity.Id;

                return response;
            }
        }
    }
}
