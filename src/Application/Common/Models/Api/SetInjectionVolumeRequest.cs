// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Application.Common.Models.Api
{
    public class SetInjectionVolumeRequest
    {
        public StreamVolumeFormat Format { get; set; }

        public double Value { get; set; }
    }
}
