// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Config;
using Application.Common.Models;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Participant.Commands
{
    public class AddParticipantStream
    {
        public class AddParticipantStreamCommand : IRequest<AddParticipantStreamCommandResponse>
        {
            public ParticipantStreamModel Participant { get; set; }
        }

        public class AddParticipantStreamCommandResponse
        {
            public string Id { get; set; }
        }

        // TODO: Analyze if we should add a validator
        public class AddParticipantStreamCommandHandler : IRequestHandler<AddParticipantStreamCommand, AddParticipantStreamCommandResponse>
        {
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly IMapper _mapper;
            private readonly IAppConfiguration _appConfiguartion;

            public AddParticipantStreamCommandHandler(
                IParticipantStreamRepository participantStreamRepository,
                IMapper mapper,
                IAppConfiguration appConfiguartion)
            {
                _participantStreamRepository = participantStreamRepository;
                _mapper = mapper;
                _appConfiguartion = appConfiguartion;
            }

            public async Task<AddParticipantStreamCommandResponse> Handle(AddParticipantStreamCommand request, CancellationToken cancellationToken)
            {
                AddParticipantStreamCommandResponse response = new AddParticipantStreamCommandResponse();

                var entity = _mapper.Map<ParticipantStream>(request.Participant);
                entity.PhotoUrl = $"https://{_appConfiguartion.BotConfiguration.MainApiUrl}/api/participant/photo/{entity.AadId}";

                await _participantStreamRepository.AddItemAsync(entity);

                response.Id = entity.Id;

                return response;
            }
        }
    }
}
