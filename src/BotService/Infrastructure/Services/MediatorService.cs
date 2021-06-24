using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Communications.Calls;
using System.Threading.Tasks;
using static Application.Call.Commands.SetCallAsEstablished;
using static Application.Call.Commands.SetCallAsTerminated;
using static Application.Service.Commands.SetBotServiceAsAvailable;
using static Application.Participant.Commands.AddParticipantStream;
using static Application.Participant.Commands.HandleParticipantLeave;
using static Application.Participant.Commands.UpdateParticipantMeetingStatus;
using Application.Common.Models;
using static Application.Service.Commands.RegisterService;

namespace BotService.Infrastructure.Services
{
    public class MediatorService : IMediatorService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public MediatorService(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<UpdateParticipantMeetingStatusCommandResponse> UpdateParticipantMeetingStatusAsync(string callId, IParticipant participant)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            var command = mapper.Map<UpdateParticipantMeetingStatusCommand>(participant);
            command.CallId = callId;

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<AddParticipantStreamCommandResponse> AddParticipantStreamAsync(string callId, IParticipant participant)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            var model = mapper.Map<ParticipantStreamModel>(participant);
            model.CallId = callId;

            var command = new AddParticipantStreamCommand
            {
                Participant = model
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<SetCallAsEstablishedCommandResponse> SetCallAsEstablishedAsync(string callId, string graphCallId)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var command = new SetCallAsEstablishedCommand
            {
                CallId = callId,
                GraphCallId = graphCallId
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<SetCallAsTerminatedCommandResponse> SetCallAsTerminatedAsync(string callId)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var command = new SetCallAsTerminatedCommand
            {
                CallId = callId
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<SetBotServiceAsAvailableCommandResponse> SetBotServiceAsAvailableAsync(string callId)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var command = new SetBotServiceAsAvailableCommand
            {
                CallId = callId
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<HandleParticipantLeaveCommandResponse> HandleParticipantLeaveAsync(string callId, string participantId)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var command = new HandleParticipantLeaveCommand
            {
                CallId = callId,
                ParticipantId = participantId
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }

        public async Task<RegisterServiceCommandResponse> RegisterServiceAsync(string virtualMachineName)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var command = new RegisterServiceCommand
            {
                VirtualMachineName = virtualMachineName
            };

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }
    }
}
