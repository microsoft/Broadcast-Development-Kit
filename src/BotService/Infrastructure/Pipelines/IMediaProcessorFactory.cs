using BotService.Application.Core;

namespace BotService.Infrastructure.Pipelines
{
    public interface IMediaProcessorFactory
    {
        IMediaProcessor CreateMediaProcessor(ProtocolSettings protocolSettings);
    }
}
