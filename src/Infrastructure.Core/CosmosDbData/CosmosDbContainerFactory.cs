using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData
{
    public class CosmosDbContainerFactory : ICosmosDbContainerFactory
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseName;
        private readonly Dictionary<string, string> _containerDictionary;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="cosmosClient"></param>
        /// <param name="databaseName"></param>
        /// <param name="containers"></param>
        public CosmosDbContainerFactory(CosmosClient cosmosClient, string databaseName)
        {
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));

            // Populate container dictionary
            _containerDictionary = new Dictionary<string, string>
            {
                { CosmosDbConstants.AuditContainer, CosmosDbConstants.AuditPartitionKey },
                { CosmosDbConstants.CallContainer, CosmosDbConstants.CallPartitionKey },
                { CosmosDbConstants.ParticipantStreamContainer, CosmosDbConstants.ParticipantStreamPartitionKey },
                { CosmosDbConstants.StreamContainer, CosmosDbConstants.StreamPartitionKey },
                { CosmosDbConstants.ServiceContainer, CosmosDbConstants.ServicePartitionKey }
            };
        }

        public ICosmosDbContainer GetContainer(string containerName)
        {
            if (!_containerDictionary.ContainsKey(containerName))
            {
                throw new ArgumentException($"Unable to find container: {containerName}");
            }

            return new CosmosDbContainer(_cosmosClient, _databaseName, containerName);
        }

        public async Task EnsureDbSetupAsync()
        {
            DatabaseResponse database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);

            foreach (var container in _containerDictionary)
            {
                await database.Database.CreateContainerIfNotExistsAsync(container.Key, container.Value);
            }
        }
    }
}
