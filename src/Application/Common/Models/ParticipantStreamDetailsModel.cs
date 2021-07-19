// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Domain.Enums;

namespace Application.Common.Models
{
    public class ParticipantStreamDetailsModel
    {
        public string StreamUrl { get; set; }

        public bool AudioDemuxed { get; set; }

        public string Passphrase { get; set; }

        public KeyLengthValues KeyLength { get; set; }

        public int Latency { get; set; }

        public string PreviewUrl { get; set; }
    }
}