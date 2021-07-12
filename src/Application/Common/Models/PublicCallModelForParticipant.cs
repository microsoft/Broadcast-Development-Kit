// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using Domain.Enums;

namespace Application.Common.Models
{
    public class PublicCallModelForParticipant
    {
        public CallState State { get; set; }

        public StreamState StreamState { get; set; }

        public Dictionary<string, string> PublicContext { get; set; } = new Dictionary<string, string>();
    }
}
