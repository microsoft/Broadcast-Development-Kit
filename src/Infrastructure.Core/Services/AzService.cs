using Application.Common.Config;
using Application.Interfaces.Common;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Core.Services
{
    public class AzService : IAzService
    {
        private readonly AzServicePrincipalConfiguration config;

        public AzService(IAppConfiguration config)
        {
            this.config = config.AzServicePrincipalConfiguration;
        }

        public IAzure GetAzure()
        {
            var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(config.ApplicationClientId, config.ApplicationClientSecret, config.TenantId, AzureEnvironment.AzureGlobalCloud);
            var azureConfiguration = Microsoft.Azure.Management.Fluent.Azure.Configure()
                .Authenticate(azureCredentials)
                .WithSubscription(config.SubscriptionId);

            return azureConfiguration;
        }
    }
}
