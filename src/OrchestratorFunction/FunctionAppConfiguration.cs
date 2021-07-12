// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Application.Common.Config;
using Microsoft.Extensions.Configuration;

namespace BotOrchestrator
{
    internal class FunctionAppConfiguration : IAppConfiguration
    {
        private readonly IConfiguration _configuration;

        public FunctionAppConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
            BuildVersion = GetEnvironmentVariable("BuildVersion");
        }

        public static string ApplicationInsightsKey => GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

        public string BuildVersion { get; set; }

        public AzStorageConfiguration StorageConfiguration => _configuration.GetSection("StorageConfiguration").Get<AzStorageConfiguration>();

        public CosmosDbConfiguration CosmosDbConfiguration => _configuration.GetSection("CosmosDbConfiguration").Get<CosmosDbConfiguration>();

        public BotConfiguration BotConfiguration { get; set; } = new BotConfiguration();

        public AzServicePrincipalConfiguration AzServicePrincipalConfiguration => _configuration.GetSection("AzServicePrincipalConfiguration").Get<AzServicePrincipalConfiguration>();

        public GraphClientConfiguration GraphClientConfiguration { get; set; } = new GraphClientConfiguration();

        public AzureAdConfiguration AzureAdConfiguration => _configuration.GetSection("AzureAdConfiguration").Get<AzureAdConfiguration>();

        public BotServiceAuthenticationConfiguration BotServiceAuthenticationConfiguration => _configuration.GetSection("BotServiceAuthenticationConfiguration").Get<BotServiceAuthenticationConfiguration>();

        private static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}