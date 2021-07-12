// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Application.Common.Models
{
    public class InfrastructureModel
    {
        public string VirtualMachineName { get; set; }

        public string ResourceGroup { get; set; }

        public string SubscriptionId { get; set; }

        public string Id { get; set; }

        public string PowerState { get; set; }

        public string IpAddress { get; set; }

        public string Dns { get; set; }

        public ProvisioningDetailsModel ProvisioningDetails { get; set; } = new ProvisioningDetailsModel();
    }
}
