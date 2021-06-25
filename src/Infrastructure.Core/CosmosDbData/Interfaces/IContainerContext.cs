using Domain.Entities.Base;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData.Interfaces
{
    /// <summary>
    ///  Defines the container level context.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IContainerContext<in T>
        where T : BaseEntity
    {
        string ContainerName { get; }

        string GenerateId(T entity);

        PartitionKey ResolvePartitionKey(string entityId);
    }
}
