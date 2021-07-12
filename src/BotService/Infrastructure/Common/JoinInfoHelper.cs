// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Infrastructure.Core.Common;
using Microsoft.Graph;

namespace BotService.Infrastructure.Common
{
    /// <summary>
    /// Gets the join information.
    /// </summary>
    public static class JoinInfoHelper
    {
        /// <summary>
        /// Parse Join URL into its components.
        /// </summary>
        /// <param name="joinUrl">Join URL from Team's meeting body.</param>
        /// <returns>Parsed data.</returns>
        public static (ChatInfo, MeetingInfo) ParseJoinURL(string joinUrl)
        {
            var meetingUrlHelper = new MeetingUrlHelper();
            meetingUrlHelper.Init(joinUrl);

            var context = meetingUrlHelper.GetContext();
            var thread = meetingUrlHelper.GetThreadId();
            var message = meetingUrlHelper.GetMessageId();

            var chatInfo = new ChatInfo
            {
                ThreadId = thread,
                MessageId = message,
                ReplyChainMessageId = context.MessageId,
            };

            var meetingInfo = new OrganizerMeetingInfo
            {
                Organizer = new IdentitySet
                {
                    User = new Identity { Id = context.Oid },
                },
            };

            meetingInfo.Organizer.User.SetTenantId(context.Tid);

            return (chatInfo, meetingInfo);
        }
    }
}
