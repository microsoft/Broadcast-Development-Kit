using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData.Interfaces
{
    public interface ICosmosDbContainer
    {
        Container Container { get; }
    }
}
