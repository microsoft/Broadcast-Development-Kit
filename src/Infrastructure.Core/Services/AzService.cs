// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Application.Common.Config;
using Application.Interfaces.Common;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Core.Services
{
    public class AzService : IAzService
    {
        private readonly AzServicePrincipalConfiguration _config;

        public AzService(IAppConfiguration config)
        {
            _config = config.AzServicePrincipalConfiguration;
        }

        public IAzure GetAzure()
        {
            var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(_config.ApplicationClientId, _config.ApplicationClientSecret, _config.TenantId, AzureEnvironment.AzureGlobalCloud);
            var azureConfiguration = Microsoft.Azure.Management.Fluent.Azure.Configure()
                .Authenticate(azureCredentials)
                .WithSubscription(_config.SubscriptionId);

            return azureConfiguration;
        }
    }
}
