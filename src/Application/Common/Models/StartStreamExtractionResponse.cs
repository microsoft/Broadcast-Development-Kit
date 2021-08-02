// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Application.Common.Models
{
    public class StartStreamExtractionResponse
    {
        public Protocol Protocol { get; set; }

        public SupportedAudioFormat AudioFormat { get; set; }

        public bool TimeOverlay { get; set; }

        public int Port { get; set; }
    }
}
