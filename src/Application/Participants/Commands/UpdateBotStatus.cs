// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Participant.Commands
{
    public class UpdateBotStatus
    {
        public class UpdateBotStatusCommand : IRequest<UpdateBotStatusResponse>
        {
            public string CallId { get; set; }

            public bool IsBotMuted { get; set; }
        }

        public class UpdateBotStatusResponse
        {
            public string Id { get; set; }

            public bool IsBotMuted { get; set; }
        }

        public class UpdateBotStatusCommandHandler : IRequestHandler<UpdateBotStatusCommand, UpdateBotStatusResponse>
        {
            private readonly ICallRepository _callRepository;
            private readonly ILogger<UpdateBotStatusCommandHandler> _logger;

            public UpdateBotStatusCommandHandler(ICallRepository callRepository, ILogger<UpdateBotStatusCommandHandler> logger)
            {
                _callRepository = callRepository;
                _logger = logger;
            }

            public async Task<UpdateBotStatusResponse> Handle(UpdateBotStatusCommand command, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(command.CallId);

                if (call.Id == null)
                {
                    _logger.LogInformation("Call {command.CallId} was not found", command.CallId);
                    throw new EntityNotFoundException(nameof(Call), command.CallId);
                }

                var response = new UpdateBotStatusResponse()
                {
                    Id = call.Id,
                };

                call.IsBotMuted = command.IsBotMuted;
                await _callRepository.UpdateItemAsync(call.Id, call);
                return response;
            }
        }
    }
}
