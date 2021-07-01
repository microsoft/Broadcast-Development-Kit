// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
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
            private readonly ILogger<SetCallAsTerminatedCommandHandler> _logger;

            public SetCallAsTerminatedCommandHandler(
                ICallRepository callRepository,
                ILogger<SetCallAsTerminatedCommandHandler> logger)
            {
                _callRepository = callRepository;
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

                response.Id = entity.Id;

                return response;
            }
        }
    }
}
