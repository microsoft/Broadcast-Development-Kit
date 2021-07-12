// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Domain.Enums;

namespace Domain.Entities.Parts
{
    public class ProvisioningDetails
    {
        public string Message { get; set; }

        public ProvisioningStateType State { get; set; }
    }
}
