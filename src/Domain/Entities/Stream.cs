using Domain.Entities.Base;
using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class Stream: BaseEntity
    {
        public string CallId { get; set; }
        public StreamState State { get; set; }
        public DateTime StartingAt { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndingAt { get; set; }
        public DateTime EndedAt { get; set; }
        public StreamDetails Details { get; set; } = new StreamDetails();
        public StreamErrorDetails Error { get; set; }
    }
}
