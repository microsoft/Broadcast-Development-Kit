using Application.Interfaces.Persistance;
using Domain.Entities;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;
using System;

namespace Infrastructure.Core.CosmosDbData.Repository
{
    public class CallRepository : CosmosDbRepository<Call>, ICallRepository
    {
        public override string ContainerName => CosmosDbConstants.CallContainer;

        public override string GenerateId(Call entity) => $"{Guid.NewGuid()}";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey(entityId);
        
        public CallRepository(ICosmosDbContainerFactory factory) : base(factory)
        { }
    }
}
