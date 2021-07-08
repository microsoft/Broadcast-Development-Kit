// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Application.Common.Models
{
    public class CallModel
    {
        public string Id { get; set; }

        public string MeetingUrl { get; set; }

        public string MeetingId { get; set; }

        public CallState State { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime EndedAt { get; set; }

        public MeetingType MeetingType { get; set; }

        public string BotFqdn { get; set; }

        public string BotIp { get; set; }

        public string DefaultPassphrase { get; set; }

        public int DefaultLatency { get; set; }

        public string GraphId { get; set; }

        public List<ParticipantStreamModel> Streams { get; set; } = new List<ParticipantStreamModel>();

        public StreamModel InjectionStream { get; set; }

        public Dictionary<string, string> PublicContext { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> PrivateContext { get; set; } = new Dictionary<string, string>();
    }
}
