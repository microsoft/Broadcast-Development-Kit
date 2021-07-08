// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Domain.Entities;
using Domain.Entities.Parts;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using static Domain.Constants.Constants;

namespace Application.Call.Commands
{
    public class SetCallAsEstablished
    {
        public class SetCallAsEstablishedCommand : IRequest<SetCallAsEstablishedCommandResponse>
        {
            public string CallId { get; set; }

            public string GraphCallId { get; set; }
        }

        public class SetCallAsEstablishedCommandResponse
        {
            public string Id { get; set; }
        }

        public class SetCallAsEstablishedCommandValidator : AbstractValidator<SetCallAsEstablishedCommand>
        {
            public SetCallAsEstablishedCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
                RuleFor(x => x.GraphCallId)
                    .NotEmpty();
            }
        }

        public class SetCallAsEstablishedCommandHandler : IRequestHandler<SetCallAsEstablishedCommand, SetCallAsEstablishedCommandResponse>
        {
            private readonly ICallRepository _callRepository;
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly ILogger<SetCallAsEstablishedCommandHandler> _logger;

            private readonly List<ParticipantStream> defaultParticipantStreams = new List<ParticipantStream>
            {
                new ParticipantStream
                {
                    ParticipantGraphId = Guid.NewGuid().ToString(),
                    DisplayName = DefaultParticipantsDisplayNames.PrimarySpeaker,
                    Type = ResourceType.PrimarySpeaker,
                    State = StreamState.Disconnected,
                    IsHealthy = true,
                    Details = new ParticipantStreamDetails(),
                },
                new ParticipantStream
                {
                    ParticipantGraphId = Guid.NewGuid().ToString(),
                    DisplayName = DefaultParticipantsDisplayNames.ScreenShare,
                    Type = ResourceType.Vbss,
                    State = StreamState.Disconnected,
                    IsHealthy = true,
                    Details = new ParticipantStreamDetails(),
                },
            };

            public SetCallAsEstablishedCommandHandler(
                ICallRepository callRepository,
                IParticipantStreamRepository participantStreamRepository,
                ILogger<SetCallAsEstablishedCommandHandler> logger)
            {
                _callRepository = callRepository;
                _participantStreamRepository = participantStreamRepository;
                _logger = logger;
            }

            public async Task<SetCallAsEstablishedCommandResponse> Handle(SetCallAsEstablishedCommand request, CancellationToken cancellationToken)
            {
                var response = new SetCallAsEstablishedCommandResponse();

                var entity = await _callRepository.GetItemAsync(request.CallId);
                if (entity == null)
                {
                    _logger.LogError("Call with id {id} was not found", request.CallId);
                    throw new EntityNotFoundException($"Call with id  {request.CallId} was not found");
                }

                entity.State = CallState.Established;
                entity.StartedAt = DateTime.UtcNow;
                entity.GraphId = request.GraphCallId;

                await _callRepository.UpdateItemAsync(entity.Id, entity);
                await AddDefaultParticipantsStreams(request.CallId);

                response.Id = entity.Id;

                return response;
            }

            private async Task AddDefaultParticipantsStreams(string callId)
            {
                var insertTasks = new List<Task>();
                foreach (var participant in defaultParticipantStreams)
                {
                    participant.CallId = callId;
                    insertTasks.Add(_participantStreamRepository.AddItemAsync(participant));
                }

                await Task.WhenAll(insertTasks);
            }
        }
    }
}
