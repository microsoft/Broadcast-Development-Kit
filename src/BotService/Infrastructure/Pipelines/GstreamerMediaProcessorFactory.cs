// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using BotService.Application.Core;
using Microsoft.Extensions.Logging;

namespace BotService.Infrastructure.Pipelines
{
    public class GStreamerMediaProcessorFactory : IMediaProcessorFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly GstreamerClockProvider _clockProvider;

        public GStreamerMediaProcessorFactory(
            ILoggerFactory loggerFactory,
            GstreamerClockProvider clockProvider)
        {
            _loggerFactory = loggerFactory;
            _clockProvider = clockProvider;
        }

        public IMediaProcessor CreateMediaProcessor(ProtocolSettings protocolSettings)
        {
            return new GstreamerMediaProcessor(protocolSettings, _loggerFactory, _clockProvider);
        }
    }
}
