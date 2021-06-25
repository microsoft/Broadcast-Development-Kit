using System.Threading.Tasks;

namespace Infrastructure.Core.CosmosDbData.Interfaces
{
    public interface ICosmosDbContainerFactory
    {
        /// <summary>
        ///     Returns a CosmosDbContainer wrapper.
        /// </summary>
        /// <param name="containerName">The name of the container in the CosmosDB account.</param>
        /// <returns>The container.</returns>
        ICosmosDbContainer GetContainer(string containerName);

        /// <summary>
        ///     Ensure the database is created.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task EnsureDbSetupAsync();
    }
}
