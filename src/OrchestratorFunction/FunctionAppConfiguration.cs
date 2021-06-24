using Application.Common.Config;
using Microsoft.Extensions.Configuration;
using System;

namespace BotOrchestrator
{
    internal class FunctionAppConfiguration: IAppConfiguration
    {
        private readonly IConfiguration configuration;

        public FunctionAppConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
            BuildVersion = GetEnvironmentVariable("BuildVersion");
        }

        public static string ApplicationInsightsKey => GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
        public string BuildVersion { get; set; }
        public AzStorageConfiguration StorageConfiguration => configuration.GetSection("StorageConfiguration").Get<AzStorageConfiguration>();
        public CosmosDbConfiguration CosmosDbConfiguration => configuration.GetSection("CosmosDbConfiguration").Get<CosmosDbConfiguration>();
        public BotConfiguration BotConfiguration { get; set; } = new BotConfiguration();
        public AzServicePrincipalConfiguration AzServicePrincipalConfiguration => configuration.GetSection("AzServicePrincipalConfiguration").Get<AzServicePrincipalConfiguration>();
        public GraphClientConfiguration GraphClientConfiguration { get; set; } = new GraphClientConfiguration();
        public AzureAdConfiguration AzureAdConfiguration => configuration.GetSection("AzureAdConfiguration").Get<AzureAdConfiguration>();
        public BotServiceAuthenticationConfiguration BotServiceAuthenticationConfiguration => configuration.GetSection("BotServiceAuthenticationConfiguration").Get<BotServiceAuthenticationConfiguration>();

        private static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}