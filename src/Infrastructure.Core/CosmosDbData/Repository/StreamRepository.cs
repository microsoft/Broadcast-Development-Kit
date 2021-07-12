// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Application.Interfaces.Persistance;
using Domain.Entities;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData.Repository
{
    public class StreamRepository : CosmosDbRepository<Stream>, IStreamRepository
    {
        public StreamRepository(ICosmosDbContainerFactory factory)
            : base(factory)
        {
        }

        public override string ContainerName => CosmosDbSchema.StreamContainer;

        public override string GenerateId(Stream entity) => $"{Guid.NewGuid()}";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey(entityId);
    }
}
