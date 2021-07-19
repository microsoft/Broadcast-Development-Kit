// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Domain.Entities.Parts
{
    public class StreamDetails
    {
        public string StreamUrl { get; set; }

        public string StreamKey { get; set; }

        public KeyLengthValues KeyLength { get; set; }

        public int Latency { get; set; }

        public string PreviewUrl { get; set; }

        public Protocol Protocol { get; set; }

        public dynamic Mode { get; set; }

        public bool EnableSsl { get; set; }
    }
}
