// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Domain.Exceptions;
using Microsoft.Graph;

namespace Infrastructure.Core.Services
{
    public class MicrosoftGraphService : IGraphService
    {
        private readonly GraphServiceClient _graphServiceClient;

        public MicrosoftGraphService(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<Call> GetCallAsync(string callId)
        {
            var call = await _graphServiceClient.Communications.Calls[callId]
                            .Request()
                            .GetAsync();

            return call;
        }

        public async Task<IList<Participant>> GetCallParticipantsAsync(string callId)
        {
            var participants = await _graphServiceClient.Communications.Calls[callId]
                .Participants
                .Request()
                .GetAsync();

            return participants;
        }

        public async Task<Stream> GetParticipantPhotoAsync(string participantAadId)
        {
            const string ErrorItemNotFound = "ErrorItemNotFound";
            const string ErrorResourceNotFound = "ResourceNotFound";

            try
            {
                // we can parametrize the requested size, or let the default size.
                var photo = await _graphServiceClient.Users[participantAadId].Photos["240x240"].Content
                    .Request()
                    .GetAsync();

                return photo;
            }
            catch (ServiceException ex) when (ex.StatusCode == HttpStatusCode.NotFound && (ex.Error.Code == ErrorItemNotFound || ex.Error.Code == ErrorResourceNotFound))
            {
                // Differentiate whether the photo doesn't exist or the participant doesn't exist.
                switch (ex.Error.Code)
                {
                    case ErrorItemNotFound:
                        string itemNotFoundMessage = $"Photo of participantAadId {participantAadId} was not found. Details: {ex.Error.Message}";
                        throw new EntityNotFoundException(itemNotFoundMessage, ex.InnerException);
                    case ErrorResourceNotFound:
                        string resourceNotFoundMessage = $"Resource participantAadId {participantAadId} was not found. Details: {ex.Error.Message}";
                        throw new EntityNotFoundException(resourceNotFoundMessage, ex.InnerException);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public async Task<IList<IList<ProfilePhoto>>> GetParticipantsPhotoAsync(IList<Participant> participants)
        {
            var tasks = new List<Task<IList<ProfilePhoto>>>();

            foreach (var participant in participants)
            {
                var user = participant.Info?.Identity?.User;
                if (user != null)
                {
                    tasks.Add(GetUserPhotoAsync(user.Id));
                }
            }

            var result = (await Task.WhenAll(tasks)).ToList();

            return result;
        }

        public async Task<IList<ProfilePhoto>> GetUserPhotoAsync(string userId)
        {
            var photo = await _graphServiceClient.Users[userId].Photos
                .Request()
                .GetAsync();

            return photo;
        }

        public async Task<OnlineMeeting> GetOnlineMeetingAsync(string meetingId)
        {
            var meetingRequest = _graphServiceClient.Me.OnlineMeetings[meetingId].Request();

            var meeting = await meetingRequest.GetAsync().ConfigureAwait(false);

            return meeting;
        }
    }
}
