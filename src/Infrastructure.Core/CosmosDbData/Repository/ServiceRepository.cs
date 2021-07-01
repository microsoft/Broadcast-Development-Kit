using System;
using Application.Interfaces.Persistance;
using Domain.Entities;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData.Repository
{
    public class ServiceRepository : CosmosDbRepository<Service>, IServiceRepository
    {
        public ServiceRepository(ICosmosDbContainerFactory factory)
            : base(factory)
        {
        }

        public override string ContainerName => CosmosDbSchema.ServiceContainer;

        public override string GenerateId(Service entity) => Guid.NewGuid().ToString();

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey(entityId);
    }
}
