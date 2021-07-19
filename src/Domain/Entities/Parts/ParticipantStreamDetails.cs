// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Domain.Entities.Parts
{
    public class ParticipantStreamDetails
    {
        public string StreamUrl { get; set; }

        public bool AudioDemuxed { get; set; }

        public string StreamKey { get; set; }

        public KeyLengthValues KeyLength { get; set; }

        public int Latency { get; set; }

        public string PreviewUrl { get; set; }

        public SupportedAudioFormat AudioFormat { get; set; }

        public bool TimeOverlay { get; set; }
    }
}
