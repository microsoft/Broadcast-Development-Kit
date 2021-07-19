// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Application.Common.Models
{
    public class SrtStreamInjectionBody : StartStreamInjectionBody
    {
        public int Latency { get; set; }

        public SrtMode Mode { get; set; }

        public KeyLengthValues KeyLength { get; set; }
    }
}
