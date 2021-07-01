// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading.Tasks;
using Application.Call.Commands;
using Application.Participant.Commands;
using Application.Service.Commands;
using Microsoft.Graph.Communications.Calls;

namespace BotService.Infrastructure.Services
{
    public interface IMediatorService
    {
        Task<AddParticipantStream.AddParticipantStreamCommandResponse> AddParticipantStreamAsync(string callId, IParticipant participant);

        Task<HandleParticipantLeave.HandleParticipantLeaveCommandResponse> HandleParticipantLeaveAsync(string callId, string participantId);

        Task<RegisterService.RegisterServiceCommandResponse> RegisterServiceAsync(string virtualMachineName);

        Task<SetBotServiceAsAvailable.SetBotServiceAsAvailableCommandResponse> SetBotServiceAsAvailableAsync(string callId);

        Task<SetCallAsEstablished.SetCallAsEstablishedCommandResponse> SetCallAsEstablishedAsync(string callId, string graphCallId);

        Task<SetCallAsTerminated.SetCallAsTerminatedCommandResponse> SetCallAsTerminatedAsync(string callId);

        Task<UpdateParticipantMeetingStatus.UpdateParticipantMeetingStatusCommandResponse> UpdateParticipantMeetingStatusAsync(string callId, IParticipant participant);
    }
}