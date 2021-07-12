// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Application.Common.Converter;
using Domain.Enums;
using Newtonsoft.Json;

namespace Application.Common.Models
{
    [JsonConverter(typeof(StartStreamInjectionBodyConverter))]
    public class StartStreamInjectionBody
    {
        public string StreamId { get; set; }

        public string CallId { get; set; }

        public string StreamUrl { get; set; }

        public string StreamKey { get; set; }

        public Protocol Protocol { get; set; }
    }
}
