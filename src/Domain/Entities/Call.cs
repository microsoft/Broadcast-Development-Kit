using Domain.Entities.Base;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Call : BaseEntity
    {
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

        public string ServiceId { get; set; }

        public Dictionary<string, string> PublicContext { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> PrivateContext { get; set; } = new Dictionary<string, string>();
    }
}
