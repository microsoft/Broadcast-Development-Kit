// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Application.Common.Config;
using BotService.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Communications.Calls.Media;
using Microsoft.Graph.Communications.Client;
using Microsoft.Graph.Communications.Common.Telemetry;

namespace BotService.Infrastructure.Client
{
    public class GraphCommunicationsClientBuilder : IGraphCommunicationsClientBuilder
    {
        private readonly IAppConfiguration _configuration;
        private readonly IGraphLogger _graphLogger;
        private readonly ILogger _logger;

        public GraphCommunicationsClientBuilder(
            IAppConfiguration configuration,
            IGraphLogger graphLogger,
            ILogger logger)
        {
            _configuration = configuration;
            _graphLogger = graphLogger;
            _logger = logger;
        }

        public ICommunicationsClient Build(string name = null)
        {
            var clientName = name ?? GetType().Assembly.GetName().Name;

            _logger.LogInformation("Instantiating CommunicationsClientBuilder for {clientName}", clientName);
            var communicationClientBuilder = new CommunicationsClientBuilder(
                    clientName,
                    _configuration.BotConfiguration.AadAppId,
                    _graphLogger);

            _logger.LogInformation("Instantiating AuthenticationProvider for {clientName}", clientName);
            var authProvider = new AuthenticationProvider(
                _configuration.BotConfiguration.AadAppId,
                _configuration.BotConfiguration.AadAppSecret,
                _graphLogger);

            _logger.LogInformation("Setting AuthenticationProvider for {clientName}", clientName);
            communicationClientBuilder.SetAuthenticationProvider(authProvider);

            _logger.LogInformation("Setting NotificationUrl for {clientName}", clientName);
            communicationClientBuilder.SetNotificationUrl(_configuration.BotConfiguration.CallControlBaseUrl);

            _logger.LogInformation("Getting MediaPlatformSettings for {clientName}", clientName);
            var mediaPlatformSettings = _configuration.BotConfiguration.GetMediaPlatformSettings();

            _logger.LogInformation("Setting MediaPlatformSettings for {clientName}", clientName);
            communicationClientBuilder.SetMediaPlatformSettings(mediaPlatformSettings);

            _logger.LogInformation("Setting ServiceBaseUrl for {clientName}", clientName);
            communicationClientBuilder.SetServiceBaseUrl(_configuration.BotConfiguration.PlaceCallEndpointUrl);

            _logger.LogInformation("Building Communication Client for {clientName}", clientName);
            var client = communicationClientBuilder.Build();

            _logger.LogInformation("Communication Client for {clientName} has been succesfully built", clientName);

            return client;
        }
    }
}
