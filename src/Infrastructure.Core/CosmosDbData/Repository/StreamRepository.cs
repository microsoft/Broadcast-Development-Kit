using Application.Interfaces.Persistance;
using Domain.Entities;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;
using System;

namespace Infrastructure.Core.CosmosDbData.Repository
{
    public class StreamRepository : CosmosDbRepository<Stream>, IStreamRepository
    {
        public override string ContainerName => CosmosDbConstants.StreamContainer;

        public override string GenerateId(Stream entity) => $"{Guid.NewGuid()}";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey(entityId);

        public StreamRepository(ICosmosDbContainerFactory factory) : base(factory)
        { }
    }
}
