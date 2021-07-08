// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Application.Common.Config
{
    public class AzServicePrincipalConfiguration
    {
        public string ApplicationClientId { get; set; }

        public string ApplicationClientSecret { get; set; }

        public string TenantId { get; set; }

        public string SubscriptionId { get; set; }
    }
}