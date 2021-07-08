// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Application.Common.Models
{
    public class StartRtmpStreamExtractionResponse : StartStreamExtractionResponse
    {
        public string StreamUrl { get; set; }

        public string StreamKey { get; set; }
    }
}
