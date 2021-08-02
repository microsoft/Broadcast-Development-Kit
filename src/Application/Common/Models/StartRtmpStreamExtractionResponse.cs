// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Application.Common.Models
{
    public class StartRtmpStreamExtractionResponse : StartStreamExtractionResponse
    {
        public RtmpMode Mode { get; set; }

        public bool EnableSsl { get; set; }

        public string StreamUrl { get; set; }

        public string StreamKey { get; set; }
    }
}
