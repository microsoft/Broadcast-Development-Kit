using Application.Interfaces.Common;
using Domain.Enums;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Core.CosmosDbData.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDb(
            this IServiceCollection services,
            string endpointUrl,
            string primaryKey,
            string databaseName)
        {
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(endpointUrl, primaryKey);
            CosmosDbContainerFactory cosmosDbClientFactory = new CosmosDbContainerFactory(client, databaseName);

            services.AddSingleton<ICosmosDbContainerFactory>(cosmosDbClientFactory);

            return services;
        }

        public static IServiceCollection AddCosmosDb(
            this IServiceCollection services,
            string endpointUrl,
            string primaryKey,
            string databaseName,
            IHostEnvironment environment)
        {
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(endpointUrl, primaryKey);
            CosmosDbContainerFactory cosmosDbClientFactory = new CosmosDbContainerFactory(client, databaseName);

            if (environment.IsLocal())
            {
                cosmosDbClientFactory.EnsureDbSetupAsync().Wait();
            }

            services.AddSingleton<ICosmosDbContainerFactory>(cosmosDbClientFactory);

            return services;
        }
    }
}
