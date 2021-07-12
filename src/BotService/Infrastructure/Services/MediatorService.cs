// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading.Tasks;
using Application.Common.Models;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Communications.Calls;
using static Application.Call.Commands.SetCallAsEstablished;
using static Application.Call.Commands.SetCallAsTerminated;
using static Application.Participant.Commands.AddParticipantStream;
using static Application.Participant.Commands.HandleParticipantLeave;
using static Application.Participant.Commands.UpdateParticipantMeetingStatus;
using static Application.Service.Commands.RegisterService;
using static Application.Service.Commands.SetBotServiceAsAvailable;

namespace BotService.Infrastructure.Services
{
    public class MediatorService : IMediatorService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MediatorService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<UpdateParticipantMeetingStatusCommandResponse> UpdateParticipantMeetingStatusAsync(string callId, IParticipant participant)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            var command = mapper.Map<UpdateParticipantMeetingStatusCommand>(participant);
            command.CallId = callId;

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<AddParticipantStreamCommandResponse> AddParticipantStreamAsync(string callId, IParticipant participant)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            var model = mapper.Map<ParticipantStreamModel>(participant);
            model.CallId = callId;

            var command = new AddParticipantStreamCommand
            {
                Participant = model,
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<SetCallAsEstablishedCommandResponse> SetCallAsEstablishedAsync(string callId, string graphCallId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var command = new SetCallAsEstablishedCommand
            {
                CallId = callId,
                GraphCallId = graphCallId,
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<SetCallAsTerminatedCommandResponse> SetCallAsTerminatedAsync(string callId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var command = new SetCallAsTerminatedCommand
            {
                CallId = callId,
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<SetBotServiceAsAvailableCommandResponse> SetBotServiceAsAvailableAsync(string callId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var command = new SetBotServiceAsAvailableCommand
            {
                CallId = callId,
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<HandleParticipantLeaveCommandResponse> HandleParticipantLeaveAsync(string callId, string participantId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var command = new HandleParticipantLeaveCommand
            {
                CallId = callId,
                ParticipantId = participantId,
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<RegisterServiceCommandResponse> RegisterServiceAsync(string virtualMachineName)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var command = new RegisterServiceCommand
            {
                VirtualMachineName = virtualMachineName,
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }
    }
}
