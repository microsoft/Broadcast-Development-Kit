using Microsoft.Graph.Communications.Client;

namespace BotService.Infrastructure.Client
{
    public interface IGraphCommunicationsClientBuilder
    {
        ICommunicationsClient Build(string name = null);
    }
}