// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Net;
using System.Text.RegularExpressions;
using Application.Common.Models;
using BotService.Infrastructure.Common;
using Newtonsoft.Json;

namespace Infrastructure.Core.Common
{
    public class MeetingUrlHelper : IMeetingUrlHelper
    {
        private Match match;

        public void Init(string joinUrl)
        {
            string decodedUrl = WebUtility.UrlDecode(joinUrl);

            var regex = new Regex("https://teams\\.microsoft\\.com.*/(?<thread>[^/]+)/(?<message>[^/]+)\\?context=(?<context>{.*})");
            match = regex.Match(decodedUrl);

            if (!match.Success)
            {
                throw new ArgumentException($"Join URL cannot be parsed: {joinUrl}.", nameof(joinUrl));
            }
        }

        public string GetThreadId()
        {
            var threadId = match.Groups["thread"].Value;

            return threadId;
        }

        public string GetMessageId()
        {
            var messageId = match.Groups["message"].Value;

            return messageId;
        }

        public JoinUrlContext GetContext()
        {
            var context = JsonConvert.DeserializeObject<JoinUrlContext>(match.Groups["context"].Value);

            return context;
        }

        public string GetMeetingId()
        {
            var template = $"0#{match.Groups["thread"].Value}#0";

            var meetingId = Base64Encode(template);

            return meetingId;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
