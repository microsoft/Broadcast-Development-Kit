using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData
{
    public class CosmosDbContainer : ICosmosDbContainer
    {
        public CosmosDbContainer(
            CosmosClient cosmosClient,
            string databaseName,
            string containerName)
        {
            Container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public Container Container { get; }
    }
}
