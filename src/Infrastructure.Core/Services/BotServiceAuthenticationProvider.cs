using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Config;
using Application.Interfaces.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Infrastructure.Core.Services
{
    public class BotServiceAuthenticationProvider : IBotServiceAuthenticationProvider
    {
        private readonly ILogger<BotServiceAuthenticationProvider> _logger;
        private readonly IAppConfiguration _appConfiguration;
        private IConfidentialClientApplication _confidentialClientApplication;

        public BotServiceAuthenticationProvider(
            ILogger<BotServiceAuthenticationProvider> logger, 
            IAppConfiguration appConfiguration)
        {
            _logger = logger;
            _appConfiguration = appConfiguration;
        }

        public IConfidentialClientApplication BuildConfidentailClientApplication()
        {
            var tokenGenerator = ConfidentialClientApplicationBuilder.Create(_appConfiguration.BotServiceAuthenticationConfiguration.ClientId)
                   .WithClientSecret(Uri.EscapeDataString(_appConfiguration.BotServiceAuthenticationConfiguration.ClientSecret))
                   .WithAuthority($"{_appConfiguration.AzureAdConfiguration.Instance}{_appConfiguration.AzureAdConfiguration.TenantId}")
                   .Build();
            return tokenGenerator;
        }

        public async Task<string> GetTokenAsync()
        {
            if (_confidentialClientApplication == null)
            {
                _confidentialClientApplication = BuildConfidentailClientApplication();
            }

            try
            {
                IEnumerable<string> scope = new List<string> { $"api://{_appConfiguration.BotServiceAuthenticationConfiguration.BotServiceApiClientId}/.default" };
                var result = await _confidentialClientApplication.AcquireTokenForClient(scope)
                        .ExecuteAsync();

                _logger.LogInformation("BotServiceAuthenticationProvider: Generated OAuth token. Expires in: {tokenExpireTime} minutes.", result.ExpiresOn.Subtract(DateTimeOffset.UtcNow).TotalMinutes);

                return result.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring Token for Client to authenticate with BotService API");
                throw;
            }

        }
    }
}
