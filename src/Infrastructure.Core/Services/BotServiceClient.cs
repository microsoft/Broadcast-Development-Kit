// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Application.Common.Models.Api;
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

        public async Task<HttpResponseMessage> InviteBotAsync(DoInviteBot.DoInviteBotCommand command)
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

        public async Task<DoStartInjection.DoStartInjectionCommandResponse> StartInjectionAsync(DoStartInjection.DoStartInjectionCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{command.Body.CallId}/stream/start-injection");
            var response = await client.PostAsync<DoStartInjection.DoStartInjectionCommandResponse>(url, null, command);

            return response;
        }

        public async Task<HttpResponseMessage> MuteBotAsync(string callId)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{callId}/mute");
            var response = await client.PostAsync(url, null);

            return response;
        }

        public async Task<HttpResponseMessage> UnmuteBotAsync(string callId)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{callId}/unmute");
            var response = await client.PostAsync(url, null);

            return response;
        }

        public async Task<DoStopInjection.DoStopInjectionCommandResponse> StoptInjectionAsync(DoStopInjection.DoStopInjectionCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{command.CallId}/stream/{command.StreamId}/stop-injection");
            var response = await client.PostAsync<DoStopInjection.DoStopInjectionCommandResponse>(url, null, command);

            return response;
        }

        public async Task<DoHideInjection.DoHideInjectionCommandResponse> HideInjectionAsync(DoHideInjection.DoHideInjectionCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{command.CallId}/injection/hide");
            var response = await client.PostAsync<DoHideInjection.DoHideInjectionCommandResponse>(url, null, command);

            return response;
        }

        public async Task<DoDisplayInjection.DoDisplayInjectionCommandResponse> DisplayInjectionAsync(DoDisplayInjection.DoDisplayInjectionCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{command.CallId}/injection/display");
            var response = await client.PostAsync<DoDisplayInjection.DoDisplayInjectionCommandResponse>(url, null, command);

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

        public async Task<DoStopExtraction.DoStopExtractionCommandResponse> StopExtractionAsync(DoStopExtraction.DoStopExtractionCommand command)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{command.Body.CallId}/stream/stop-extraction");
            var response = await client.PostAsync<DoStopExtraction.DoStopExtractionCommandResponse>(url, null, command);

            return response;
        }

        public async Task<HttpResponseMessage> SetInjectionVolumeAsync(string callId, SetInjectionVolumeRequest setInjectionVolumeRequest)
        {
            ValidateBaseUrl();

            var client = await GetClient();
            var url = new Uri($"https://{_baseUrl}/api/bot/call/{callId}/injection/set-volume");
            var response = await client.PostAsync(url, null, setInjectionVolumeRequest);

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
