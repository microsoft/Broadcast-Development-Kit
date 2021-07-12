// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using BotService.Application.Core;
using BotService.Infrastructure.Common.Logging;
using BotService.Infrastructure.Pipelines;
using Microsoft.Extensions.Logging;
using Microsoft.Skype.Bots.Media;

namespace BotService.Infrastructure.Core
{
    public class MediaHandlerFactory : IMediaHandlerFactory
    {
        private readonly IMediaProcessorFactory _processorFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly PipelineBusObserver _pipelineBusObserver;

        public MediaHandlerFactory(IMediaProcessorFactory processorFactory, ILoggerFactory loggerFactory, PipelineBusObserver pipelineBusObserver)
        {
            _processorFactory = processorFactory;
            _loggerFactory = loggerFactory;
            _pipelineBusObserver = pipelineBusObserver;
        }

        public IMediaExtractor CreateExtractor(IVideoSocket videoSocket, IAudioSocket audioSocket)
        {
            return new MediaExtractor(videoSocket, audioSocket, _processorFactory, _loggerFactory);
        }

        public IMediaInjector CreateInjector(IVideoSocket videoSocket, IAudioSocket audioSocket)
        {
            return new MediaInjector(videoSocket, audioSocket, _loggerFactory, _pipelineBusObserver);
        }

        public ISwitchingMediaExtractor CreateSwitchingExtractor(IVideoSocket videoSocket, IMediaSocketPool mediaSocketPool, IAudioSocket audioSocket)
        {
            return new SwitchingMediaExtractor(videoSocket, mediaSocketPool, audioSocket, _processorFactory, _loggerFactory);
        }
    }
}
