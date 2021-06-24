using Application.Common.Config;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Core.Services
{
    /// <summary>
    /// This class provides the required provider to instantiate a GraphServiceClient.
    /// </summary>
    public class GraphAuthenticationProvider : IAuthenticationProvider
    {
        private const string GRAPH_URI = "https://graph.microsoft.com/";
        private readonly GraphClientConfiguration configuration;

        public GraphAuthenticationProvider(IAppConfiguration configuration)
        {
            this.configuration = configuration.GraphClientConfiguration;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            AuthenticationContext authContext = new AuthenticationContext($"https://login.microsoftonline.com/{configuration.TenantId}");

            ClientCredential creds = new ClientCredential(configuration.ClientId, configuration.ClientSecret);

            AuthenticationResult authResult = await authContext.AcquireTokenAsync(GRAPH_URI, creds);

            request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);
        }
    }
}
