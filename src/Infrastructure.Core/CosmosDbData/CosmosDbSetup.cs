// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading.Tasks;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData
{
    public class CosmosDbSetup : ICosmosDbSetup
    {
        private readonly CosmosClient _client;
        private readonly string _databaseName;

        public CosmosDbSetup(CosmosClient client, string databaseName)
        {
            _client = client;
            _databaseName = databaseName;
        }

        public async Task SetupDatabaseAsync()
        {
            DatabaseResponse database = await _client.CreateDatabaseIfNotExistsAsync(_databaseName);

            foreach (var container in CosmosDbSchema.Containers)
            {
                await database.Database.CreateContainerIfNotExistsAsync(container.Key, container.Value).ConfigureAwait(false);
            }
        }
    }
}
