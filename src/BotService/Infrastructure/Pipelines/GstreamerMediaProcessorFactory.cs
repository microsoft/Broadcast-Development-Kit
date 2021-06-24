using BotService.Application.Core;
using Microsoft.Extensions.Logging;

namespace BotService.Infrastructure.Pipelines
{
    public class GStreamerMediaProcessorFactory : IMediaProcessorFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public GStreamerMediaProcessorFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IMediaProcessor CreateMediaProcessor(ProtocolSettings protocolSettings)
        {
            return new GstreamerMediaProcessor(protocolSettings, _loggerFactory);
        }
    }
}
