using Application.Interfaces.Persistance;
using Domain.Entities;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;
using System;

namespace Infrastructure.Core.CosmosDbData.Repository
{
    public class ParticipantStreamRepository : CosmosDbRepository<ParticipantStream>, IParticipantStreamRepository
    {
        public override string ContainerName => CosmosDbConstants.ParticipantStreamContainer;

        public override string GenerateId(ParticipantStream entity) => $"{Guid.NewGuid()}";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey(entityId);

        public ParticipantStreamRepository(ICosmosDbContainerFactory factory) : base(factory)
        { }
    }
}
