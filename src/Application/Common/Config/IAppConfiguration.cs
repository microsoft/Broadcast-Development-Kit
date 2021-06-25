using Domain.Enums;

namespace Application.Common.Config
{
    public interface IAppConfiguration
    {
        string BuildVersion { get; set; }

        GraphClientConfiguration GraphClientConfiguration { get; }

        AzStorageConfiguration StorageConfiguration { get; }

        CosmosDbConfiguration CosmosDbConfiguration { get; }

        BotConfiguration BotConfiguration { get; }

        AzServicePrincipalConfiguration AzServicePrincipalConfiguration { get; }

        AzureAdConfiguration AzureAdConfiguration { get; }

        BotServiceAuthenticationConfiguration BotServiceAuthenticationConfiguration { get; }
    }
}
