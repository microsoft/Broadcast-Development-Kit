// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Application.Common.Config;
using Domain.Enums;

namespace ManagementApi
{
    public class AppConfiguration : IAppConfiguration
    {
        public string BuildVersion { get; set; }

        public GraphClientConfiguration GraphClientConfiguration { get; set; } = new GraphClientConfiguration();

        public AzStorageConfiguration StorageConfiguration { get; set; } = new AzStorageConfiguration();

        public CosmosDbConfiguration CosmosDbConfiguration { get; set; } = new CosmosDbConfiguration();

        public BotConfiguration BotConfiguration { get; set; } = new BotConfiguration();

        public AzServicePrincipalConfiguration AzServicePrincipalConfiguration { get; set; } = new AzServicePrincipalConfiguration();

        public AzureAdConfiguration AzureAdConfiguration { get; set; } = new AzureAdConfiguration();

        public BotServiceAuthenticationConfiguration BotServiceAuthenticationConfiguration { get; set; } = new BotServiceAuthenticationConfiguration();
    }
}