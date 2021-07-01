// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Config;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Infrastructure.Core.Services
{
    /// <summary>
    /// This class provides the required provider to instantiate a GraphServiceClient.
    /// </summary>
    public class GraphAuthenticationProvider : IAuthenticationProvider
    {
        private const string GraphUri = "https://graph.microsoft.com/";
        private readonly GraphClientConfiguration _configuration;

        public GraphAuthenticationProvider(IAppConfiguration configuration)
        {
            _configuration = configuration.GraphClientConfiguration;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            AuthenticationContext authContext = new AuthenticationContext($"https://login.microsoftonline.com/{_configuration.TenantId}");

            ClientCredential creds = new ClientCredential(_configuration.ClientId, _configuration.ClientSecret);

            AuthenticationResult authResult = await authContext.AcquireTokenAsync(GraphUri, creds);

            request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);
        }
    }
}
