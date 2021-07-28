// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Application.Common.Models
{
    public class RtmpStreamExtractionBody : StartStreamExtractionBody
    {
        public RtmpMode Mode { get; set; }

        public bool EnableSsl { get; set; }
    }
}
