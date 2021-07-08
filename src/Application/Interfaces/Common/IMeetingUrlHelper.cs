// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Application.Common.Models;

namespace BotService.Infrastructure.Common
{
    public interface IMeetingUrlHelper
    {
        JoinUrlContext GetContext();

        string GetMeetingId();

        string GetMessageId();

        string GetThreadId();

        void Init(string joinUrl);
    }
}