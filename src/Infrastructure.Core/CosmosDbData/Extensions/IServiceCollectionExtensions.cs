using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;
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
            CosmosClient client = new CosmosClient(endpointUrl, primaryKey);
            CosmosDbSetup.SetupDatabaseAsync(client, databaseName).Wait();

            CosmosDbContainerFactory cosmosDbClientFactory = new CosmosDbContainerFactory(client, databaseName);
            services.AddSingleton<ICosmosDbContainerFactory>(cosmosDbClientFactory);

            return services;
        }
    }
}
