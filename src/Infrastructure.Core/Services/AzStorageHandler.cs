using Application.Common.Config;
using Application.Interfaces.Common;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Core.Services
{
    public class AzStorageHandler : IAzStorageHandler
    {
        private readonly IAppConfiguration configuration;

        public AzStorageHandler(IAppConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task AddQueueMessageAsync(string queue, object message)
        {
            var plainText = JsonConvert.SerializeObject(message);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            var queueMessage = Convert.ToBase64String(plainTextBytes);

            var queueClient = new QueueClient(configuration.StorageConfiguration.ConnectionString, queue);

            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(queueMessage);
        }
    }
}
