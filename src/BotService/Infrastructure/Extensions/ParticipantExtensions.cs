// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Linq;
using Application.Common.Models;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Calls;
using static Domain.Constants.Constants;

namespace BotService.Infrastructure.Extensions
{
    public static class ParticipantExtensions
    {
        private const string GuestKey = "guest";
        private const string ApplicationTypeKey = "ApplicationType";
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

        public static bool IsTogetherModeBot(this IParticipant participant)
        {
            bool isTogetherModeBot = IsMeetingBot(participant, MicrosoftTeamsBotApplicationType.TogetherMode);

            return isTogetherModeBot;
        }

        public static bool IsLargeGalleryBot(this IParticipant participant)
        {
            bool isLargeGalleryBot = IsMeetingBot(participant, MicrosoftTeamsBotApplicationType.LargeGallery);

            return isLargeGalleryBot;
        }

        public static bool IsAnAllowedParticipant(this IParticipant participant)
        {
            bool isAnAllowedParticipant = participant.IsUser() ||
                participant.IsGuestUser() ||
                participant.IsLiveEventBot() ||
                participant.IsTogetherModeBot() ||
                participant.IsLargeGalleryBot();

            return isAnAllowedParticipant;
        }

        public static ResourceIdentity GetUserIdentity(this IParticipant participant)
        {
            ResourceIdentity identity = new ResourceIdentity();
            if (participant.IsUser())
            {
                var userIdentity = participant.Resource.Info.Identity.User;
                identity.Id = userIdentity.Id;
                identity.DisplayName = userIdentity.DisplayName;
            }

            if (participant.IsGuestUser())
            {
                var guestUserIdentity = participant.Resource.Info.Identity.AdditionalData[GuestKey] as Identity;
                identity.Id = guestUserIdentity.Id;
                identity.DisplayName = guestUserIdentity.DisplayName;
            }

            if (participant.IsLiveEventBot())
            {
                var liveEventBotIdentity = participant.Resource.Info.Identity.Application;
                identity.Id = liveEventBotIdentity.Id;
                identity.DisplayName = liveEventBotIdentity.DisplayName;
            }

            if (participant.IsTogetherModeBot())
            {
                var togetherModeBotIdentity = participant.Resource.Info.Identity.Application;
                identity.Id = togetherModeBotIdentity.Id;
                identity.DisplayName = "Together Mode";
            }

            if (participant.IsLargeGalleryBot())
            {
                var largeGalleryBotIdentity = participant.Resource.Info.Identity.Application;
                identity.Id = largeGalleryBotIdentity.Id;
                identity.DisplayName = "Large Gallery";
            }

            return identity;
        }

        private static bool IsMeetingBot(IParticipant participant, string type)
        {
            bool isAnApplication = participant.Resource.Info.Identity.Application != null
               && participant.Resource.Info.Identity.Application.AdditionalData.ContainsKey(ApplicationTypeKey);

            if (!isAnApplication)
            {
                return false;
            }

            var applicationType = participant.Resource.Info.Identity.Application.AdditionalData[ApplicationTypeKey] as string;

            // Together mode and Large gallery mode have two bots, one of them doesn't have the capability to send video
            // so we don't want to add it as a participant.
            bool isMeetingBot = applicationType == type && participant.IsParticipantCapableToSendVideo();

            return isMeetingBot;
        }
    }
}
