// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData
{
    public static class CosmosDbSetup
    {
        public static async Task SetupDatabaseAsync(CosmosClient client, string databaseName)
        {
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);

            foreach (var container in CosmosDbSchema.Containers)
            {
                await database.Database.CreateContainerIfNotExistsAsync(container.Key, container.Value).ConfigureAwait(false);
            }
        }
    }
}
