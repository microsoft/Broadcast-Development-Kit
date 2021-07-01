// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Application.Interfaces.Common
{
    public interface IGraphService
    {
        Task<Microsoft.Graph.Call> GetCallAsync(string callId);

        Task<IList<Microsoft.Graph.Participant>> GetCallParticipantsAsync(string callId);

        Task<IList<ProfilePhoto>> GetUserPhotoAsync(string userId);

        Task<System.IO.Stream> GetParticipantPhotoAsync(string participantAadId);

        Task<IList<IList<ProfilePhoto>>> GetParticipantsPhotoAsync(IList<Microsoft.Graph.Participant> participants);

        Task<OnlineMeeting> GetOnlineMeetingAsync(string meetingId);
    }
}
