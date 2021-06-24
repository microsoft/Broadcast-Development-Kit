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
        private readonly IAppConfiguration configuration;
        private readonly IGraphLogger graphLogger;
        private readonly ILogger logger;

        public GraphCommunicationsClientBuilder(
            IAppConfiguration configuration,
            IGraphLogger graphLogger,
            ILogger logger
            )
        {
            this.configuration = configuration;
            this.graphLogger = graphLogger;
            this.logger = logger;
        }

        public ICommunicationsClient Build(string name = null)
        {
            var clientName = name ?? this.GetType().Assembly.GetName().Name;

            logger.LogInformation("Instantiating CommunicationsClientBuilder for {clientName}", clientName);
            var communicationClientBuilder = new CommunicationsClientBuilder(
                    clientName,
                    configuration.BotConfiguration.AadAppId,
                    graphLogger);

            logger.LogInformation("Instantiating AuthenticationProvider for {clientName}", clientName);
            var authProvider = new AuthenticationProvider(
                configuration.BotConfiguration.AadAppId,
                configuration.BotConfiguration.AadAppSecret,
                graphLogger);

            logger.LogInformation("Setting AuthenticationProvider for {clientName}", clientName);
            communicationClientBuilder.SetAuthenticationProvider(authProvider);

            logger.LogInformation("Setting NotificationUrl for {clientName}", clientName);
            communicationClientBuilder.SetNotificationUrl(configuration.BotConfiguration.CallControlBaseUrl);

            logger.LogInformation("Getting MediaPlatformSettings for {clientName}", clientName);
            var mediaPlatformSettings = configuration.BotConfiguration.GetMediaPlatformSettings();

            logger.LogInformation("Setting MediaPlatformSettings for {clientName}", clientName);
            communicationClientBuilder.SetMediaPlatformSettings(mediaPlatformSettings);

            logger.LogInformation("Setting ServiceBaseUrl for {clientName}", clientName);
            communicationClientBuilder.SetServiceBaseUrl(configuration.BotConfiguration.PlaceCallEndpointUrl);

            logger.LogInformation("Building Communication Client for {clientName}", clientName);
            var client = communicationClientBuilder.Build();

            logger.LogInformation("Communication Client for {clientName} has been succesfully built", clientName);

            return client;
        }
    }
}
