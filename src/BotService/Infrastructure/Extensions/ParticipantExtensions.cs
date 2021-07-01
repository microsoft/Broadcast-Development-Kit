// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Linq;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Calls;

namespace BotService.Infrastructure.Extensions
{
    public static class ParticipantExtensions
    {
        private const string GuestKey = "guest";
        private const string LiveEventBot = "live";

        public static bool IsParticipantCapableToSendVideo(this IParticipant participant)
        {
            bool isCapable = participant.Resource.MediaStreams.Any(x => x.MediaType == Modality.Video &&
               (x.Direction == MediaDirection.SendReceive || x.Direction == MediaDirection.SendOnly));

            return isCapable;
        }

        public static bool IsParticipantCapableToSendAudio(this IParticipant participant)
        {
            bool isCapable = participant.Resource.MediaStreams.Any(x => x.MediaType == Modality.Audio &&
               (x.Direction == MediaDirection.SendReceive || x.Direction == MediaDirection.SendOnly));

            return isCapable;
        }

        public static bool IsParticipantSharingScreen(this IParticipant participant)
        {
            bool isSharingScreen = participant.Resource.MediaStreams.Any(stream => stream.MediaType == Modality.VideoBasedScreenSharing &&
                (stream.Direction == MediaDirection.SendReceive || stream.Direction == MediaDirection.SendOnly));

            return isSharingScreen;
        }

        public static MediaStream GetParticipantStream(this IParticipant participant)
        {
            return participant.Resource.MediaStreams.FirstOrDefault(x => x.MediaType == Modality.Video &&
                (x.Direction == MediaDirection.SendReceive || x.Direction == MediaDirection.SendOnly));
        }

        public static MediaStream GetScreenShareStream(this IParticipant participant)
        {
            return participant.Resource.MediaStreams.FirstOrDefault(stream => stream.MediaType == Modality.VideoBasedScreenSharing &&
                (stream.Direction == MediaDirection.SendReceive || stream.Direction == MediaDirection.SendOnly));
        }

        public static bool IsUser(this IParticipant participant)
        {
            bool isUser = participant.Resource.Info.Identity.User != null;

            return isUser;
        }

        public static bool IsGuestUser(this IParticipant participant)
        {
            bool isGuestUser = participant.Resource.Info.Identity.AdditionalData.ContainsKey(GuestKey);

            return isGuestUser;
        }

        public static bool IsLiveEventBot(this IParticipant participant)
        {
            bool isLiveEventBot = participant.Resource.Info.Identity.Application != null && participant.Resource.Info.Identity.Application.DisplayName?.ToLower() == LiveEventBot;

            return isLiveEventBot;
        }

        public static Identity GetUserIdentity(this IParticipant participant)
        {
            if (participant.IsUser())
            {
                return participant.Resource.Info.Identity.User;
            }

            if (participant.IsGuestUser())
            {
                return participant.Resource.Info.Identity.AdditionalData[GuestKey] as Identity;
            }

            if (participant.IsLiveEventBot())
            {
                return participant.Resource.Info.Identity.Application;
            }

            return null;
        }
    }
}
