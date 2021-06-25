using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace BotService.Infrastructure.Client
{
    public class AuthenticationProvider : ObjectRoot, IRequestAuthenticationProvider
    {
        /// <summary>
        /// The application identifier.
        /// </summary>
        private readonly string _appId;

        /// <summary>
        /// The application secret.
        /// </summary>
        private readonly string _appSecret;

        /// <summary>
        /// The open ID configuration refresh interval.
        /// </summary>
        private readonly TimeSpan _openIdConfigRefreshInterval = TimeSpan.FromHours(2);

        /// <summary>
        /// The previous update timestamp for OpenIdConfig.
        /// </summary>
        private DateTime _prevOpenIdConfigUpdateTimestamp = DateTime.MinValue;

        /// <summary>
        /// The open identifier configuration.
        /// </summary>
        private OpenIdConnectConfiguration _openIdConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationProvider" /> class.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <param name="appSecret">The application secret.</param>
        /// <param name="logger">The logger.</param>
        public AuthenticationProvider(string appId, string appSecret, IGraphLogger logger)
            : base(logger.NotNull(nameof(logger)).CreateShim(nameof(AuthenticationProvider)))
        {
            _appId = appId.NotNullOrWhitespace(nameof(appId));
            _appSecret = appSecret.NotNullOrWhitespace(nameof(appSecret));
        }

        /// <summary>
        /// Authenticates the specified request message.
        /// This method will be called any time there is an outbound request.
        /// In this case we are using the Microsoft.IdentityModel.Clients.ActiveDirectory library
        /// to stamp the outbound http request with the OAuth 2.0 token using an AAD application id
        /// and application secret.  Alternatively, this method can support certificate validation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="tenant">The tenant.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        public async Task AuthenticateOutboundRequestAsync(HttpRequestMessage request, string tenant)
        {
            const string schema = "Bearer";
            const string replaceString = "{tenant}";
            const string oauthV2TokenLink = "https://login.microsoftonline.com/{tenant}";
            const string resource = "https://graph.microsoft.com";

            // If no tenant was specified, we craft the token link using the common tenant.
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols#endpoints
            tenant = string.IsNullOrWhiteSpace(tenant) ? "common" : tenant;
            var tokenLink = oauthV2TokenLink.Replace(replaceString, tenant);

            GraphLogger.Info("AuthenticationProvider: Generating OAuth token.");
            var context = new AuthenticationContext(tokenLink);
            var creds = new ClientCredential(_appId, _appSecret);

            AuthenticationResult result;
            try
            {
                result = await AcquireTokenWithRetryAsync(context, resource, creds, attempts: 3).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                GraphLogger.Error(ex, $"Failed to generate token for client: {_appId}");
                throw;
            }

            GraphLogger.Info($"AuthenticationProvider: Generated OAuth token. Expires in {result.ExpiresOn.Subtract(DateTimeOffset.UtcNow).TotalMinutes} minutes.");

            request.Headers.Authorization = new AuthenticationHeaderValue(schema, result.AccessToken);
        }

        /// <summary>
        /// Validates the request asynchronously.
        /// This method will be called any time we have an incoming request.
        /// Returning invalid result will trigger a Forbidden response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The <see cref="RequestValidationResult" /> structure.
        /// </returns>
        public async Task<RequestValidationResult> ValidateInboundRequestAsync(HttpRequestMessage request)
        {
            var token = request?.Headers?.Authorization?.Parameter;
            if (string.IsNullOrWhiteSpace(token))
            {
                return new RequestValidationResult { IsValid = false };
            }

            // Currently the service does not sign outbound request using AAD, instead it is signed
            // with a private certificate.  In order for us to be able to ensure the certificate is
            // valid we need to download the corresponding public keys from a trusted source.
            const string authDomain = "https://api.aps.skype.com/v1/.well-known/OpenIdConfiguration";
            if (_openIdConfiguration == null || DateTime.Now > _prevOpenIdConfigUpdateTimestamp.Add(_openIdConfigRefreshInterval))
            {
                GraphLogger.Info("Updating OpenID configuration");

                // Download the OIDC configuration which contains the JWKS
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager =
                    new ConfigurationManager<OpenIdConnectConfiguration>(
                        authDomain,
                        new OpenIdConnectConfigurationRetriever());
                _openIdConfiguration = await configurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);

                _prevOpenIdConfigUpdateTimestamp = DateTime.Now;
            }

            // The incoming token should be issued by graph.
            var authIssuers = new[]
            {
                "https://graph.microsoft.com",
                "https://api.botframework.com",
            };

            // Configure the TokenValidationParameters.
            // Aet the Issuer(s) and Audience(s) to validate and
            // assign the SigningKeys which were downloaded from AuthDomain.
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuers = authIssuers,
                ValidAudience = _appId,
                IssuerSigningKeys = _openIdConfiguration.SigningKeys,
            };

            ClaimsPrincipal claimsPrincipal;
            try
            {
                // Now validate the token. If the token is not valid for any reason, an exception will be thrown by the method
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                claimsPrincipal = handler.ValidateToken(token, validationParameters, out _);
            }

            // Token expired... should somehow return 401 (Unauthorized)
            // Tampered token
            // Some other validation error
            catch (Exception ex)
            {
                // Some other error
                GraphLogger.Error(ex, $"Failed to validate token for client: {_appId}.");
                return new RequestValidationResult() { IsValid = false };
            }

            const string claimType = "http://schemas.microsoft.com/identity/claims/tenantid";
            var tenantClaim = claimsPrincipal.FindFirst(claim => claim.Type.Equals(claimType, StringComparison.Ordinal));

            if (string.IsNullOrEmpty(tenantClaim?.Value))
            {
                // No tenant claim given to us.  reject the request.
                return new RequestValidationResult { IsValid = false };
            }

            request.Properties.Add(HttpConstants.HeaderNames.Tenant, tenantClaim.Value);
            return new RequestValidationResult { IsValid = true, TenantId = tenantClaim.Value };
        }

        /// <summary>
        /// Acquires the token and retries if failure occurs.
        /// </summary>
        /// <param name="context">The application context.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="creds">The application credentials.</param>
        /// <param name="attempts">The attempts.</param>
        /// <returns>
        /// The <see cref="AuthenticationResult" />.
        /// </returns>
        private static async Task<AuthenticationResult> AcquireTokenWithRetryAsync(AuthenticationContext context, string resource, ClientCredential creds, int attempts)
        {
            while (true)
            {
                attempts--;

                try
                {
                    return await context.AcquireTokenAsync(resource, creds).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    if (attempts < 1)
                    {
                        throw;
                    }
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }
}
