// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Domain.Entities.Parts;
using Domain.Enums;

namespace Application.Common.Models
{
    public class ParticipantStreamModel
    {
        public string Id { get; set; }

        public string AadId { get; set; }

        public string CallId { get; set; }

        public string ParticipantGraphId { get; set; }

        public string DisplayName { get; set; }

        public string PhotoUrl { get; set; }

        public ResourceType Type { get; set; }

        public StreamState State { get; set; }

        public bool IsHealthy { get; set; }

        public string HealthMessage { get; set; }

        public bool AudioMuted { get; set; }

        public bool IsSharingAudio { get; set; }

        public bool IsSharingVideo { get; set; }

        public bool IsSharingScreen { get; set; }

        public ParticipantStreamDetailsModel Details { get; set; } = new ParticipantStreamDetailsModel();

        public DateTime CreatedAt { get; set; }

        public DateTime? LeftAt { get; set; }

        public StreamErrorDetails Error { get; set; }
    }
}
