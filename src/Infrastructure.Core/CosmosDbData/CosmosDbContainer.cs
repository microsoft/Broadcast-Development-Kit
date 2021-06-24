using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData
{
    public class CosmosDbContainer : ICosmosDbContainer
    {
        public Container Container { get; }
        
        public CosmosDbContainer(CosmosClient cosmosClient,
            string databaseName,
            string containerName)
        {
            this.Container = cosmosClient.GetContainer(databaseName, containerName);
        }
    }
}
