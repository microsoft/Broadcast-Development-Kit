// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Application.Participants.Specifications;
using Application.Stream.Specifications;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Call.Commands
{
    public class SetCallAsTerminated
    {
        public class SetCallAsTerminatedCommand : IRequest<SetCallAsTerminatedCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class SetCallAsTerminatedCommandResponse
        {
            public string Id { get; set; }
        }

        public class SetCallAsTerminatedCommandValidator : AbstractValidator<SetCallAsTerminatedCommand>
        {
            public SetCallAsTerminatedCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class SetCallAsTerminatedCommandHandler : IRequestHandler<SetCallAsTerminatedCommand, SetCallAsTerminatedCommandResponse>
        {
            private readonly ICallRepository _callRepository;
            private readonly IParticipantStreamRepository _participantRepository;
            private readonly IStreamRepository _streamRepository;
            private readonly ILogger<SetCallAsTerminatedCommandHandler> _logger;

            public SetCallAsTerminatedCommandHandler(
                ICallRepository callRepository,
                IParticipantStreamRepository participantRepository,
                IStreamRepository streamRepository,
                ILogger<SetCallAsTerminatedCommandHandler> logger)
            {
                _callRepository = callRepository;
                _participantRepository = participantRepository;
                _streamRepository = streamRepository;
                _logger = logger;
            }

            public async Task<SetCallAsTerminatedCommandResponse> Handle(SetCallAsTerminatedCommand request, CancellationToken cancellationToken)
            {
                var response = new SetCallAsTerminatedCommandResponse();

                var entity = await _callRepository.GetItemAsync(request.CallId);
                if (entity == null)
                {
                    _logger.LogError("Call with id {id} was not found", request.CallId);
                    throw new EntityNotFoundException($"Call with id  {request.CallId} was not found");
                }

                entity.State = Domain.Enums.CallState.Terminated;
                entity.EndedAt = DateTime.UtcNow;
                await _callRepository.UpdateItemAsync(entity.Id, entity);

                try
                {
                    _logger.LogInformation("Call with id {id} was set as Terminated. Removing streams...", request.CallId);

                    // Remove from the database all streams associated with this call
                    var streamsSpecification = new StreamsGetFromCallSpecification(request.CallId);
                    var streams = await _streamRepository.GetItemsAsync(streamsSpecification);
                    if (streams != null && streams.Any())
                    {
                        var deleteTasks = new List<Task>();
                        foreach (var stream in streams)
                        {
                            deleteTasks.Add(_streamRepository.DeleteItemAsync(stream.Id));
                        }

                        await Task.WhenAll(deleteTasks);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There was an error cleaning the streams from the call {id}.", request.CallId);
                }

                try
                {
                    _logger.LogInformation("Call with id {id} was set as Terminated. Removing participants...", request.CallId);

                    // Remove from the database all participants associated with this call
                    var participantsSpecification = new ParticipantsStreamsGetFromCallSpecification(request.CallId, archived: true);
                    var participants = await _participantRepository.GetItemsAsync(participantsSpecification);
                    if (participants != null && participants.Any())
                    {
                        var deleteTasks = new List<Task>();
                        foreach (var participant in participants)
                        {
                            deleteTasks.Add(_participantRepository.DeleteItemAsync(participant.Id));
                        }

                        await Task.WhenAll(deleteTasks);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There was an error cleaning the participants from the call {id}.", request.CallId);
                }

                // TODO: At the moment we are not removing the call from the database. Review this use case.
                response.Id = entity.Id;

                return response;
            }
        }
    }
}
