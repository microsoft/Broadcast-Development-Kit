using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Core.CosmosDbData.Interfaces
{
    public interface ICosmosDbContainerFactory
    {
        Container GetContainer(string containerName);
    }
}
