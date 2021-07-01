using System;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData
{
    public class CosmosDbContainerFactory : ICosmosDbContainerFactory
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseName;

        public CosmosDbContainerFactory(CosmosClient cosmosClient, string databaseName)
        {
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        }

        public Container GetContainer(string containerName)
        {
            if (!CosmosDbSchema.Containers.ContainsKey(containerName))
            {
                throw new ArgumentException($"Unknown container: {containerName}", nameof(containerName));
            }

            return _cosmosClient.GetContainer(_databaseName, containerName);
        }
    }
}
