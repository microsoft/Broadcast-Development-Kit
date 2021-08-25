// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Service.Commands;
using Application.Stream.Commands;
using Infrastructure.Core.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Core.Services
{
    public class BotServiceClient : IBotServiceClient
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<BotServiceClient> _logger;
        private readonly IBotServiceAuthenticationProvider _botServiceAuthenticationProvider;
        private readonly IHostEnvironment _environment;
        private string _baseUrl;

        public BotServiceClient(
            IHttpClientFactory clientFactory,
            ILogger<BotServiceClient> logger,
            IBotServiceAuthenticationProvider botServiceAuthenticationProvider,
            IHostEnvironment hostEnvironment)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _botServiceAuthenticationProvider = botServiceAuthenticationProvider;
            _environment = hostEnvironment;
        }

        public void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public async Task<HttpResponseMessage> InviteBotAsync(InviteBot.InviteBotCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/invite");
            var response = await client.PostAsync(url, null, command);

            return response;
        }

        public async Task<HttpResponseMessage> RemoveBotAsync(string callGraphId)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{callGraphId}");
            var response = await client.DeleteAsync(url);

            return response;
        }

        public async Task<StartInjection.StartInjectionCommandResponse> StartInjectionAsync(StartInjection.StartInjectionCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{command.Body.CallId}/stream/start-injection");
            var response = await client.PostAsync<StartInjection.StartInjectionCommandResponse>(url, null, command);

            return response;
        }

        public async Task<HttpResponseMessage> MuteBotAsync()
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/mute");
            var response = await client.PostAsync(url, null);

            return response;
        }

        public async Task<HttpResponseMessage> UnmuteBotAsync()
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/unmute");
            var response = await client.PostAsync(url, null);

            return response;
        }

        public async Task<StopInjection.StopInjectionCommandResponse> StoptInjectionAsync(StopInjection.StopInjectionCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{command.CallId}/stream/{command.StreamId}/stop-injection");
            var response = await client.PostAsync<StopInjection.StopInjectionCommandResponse>(url, null, command);

            return response;
        }

        public async Task<DoStartExtraction.DoStartExtractionCommandResponse> StartExtractionAsync(DoStartExtraction.DoStartExtractionCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{command.Body.CallId}/stream/start-extraction");
            var response = await client.PostAsync<DoStartExtraction.DoStartExtractionCommandResponse>(url, null, command);

            return response;
        }

        public async Task<StopExtraction.StopExtractionCommandResponse> StopExtractionAsync(StopExtraction.StopExtractionCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{command.Body.CallId}/stream/stop-extraction");
            var response = await client.PostAsync<StopExtraction.StopExtractionCommandResponse>(url, null, command);

            return response;
        }

        private async Task<HttpClient> GetClient()
        {
            if (_environment.IsLocal())
            {
                var client = _clientFactory.CreateClient("bot-service-client");

                return client;
            }

            return await GetClientWithAuthenticationHeader();
        }

        private async Task<HttpClient> GetClientWithAuthenticationHeader()
        {
            var client = _clientFactory.CreateClient("bot-service-client");
            var accessToken = await _botServiceAuthenticationProvider.GetTokenAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return client;
        }

        private void ValidateBaseUrl()
        {
            if (string.IsNullOrEmpty(_baseUrl))
            {
                _logger.LogError("[BroadcastBotServiceClient] Bot client base url hasn't been set");
                throw new BotClientBaseUrlNotSetException("Bot client base url hasn't been set");
            }
        }
    }
}
