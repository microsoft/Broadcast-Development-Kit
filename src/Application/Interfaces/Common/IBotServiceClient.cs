// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Net.Http;
using System.Threading.Tasks;
using Application.Service.Commands;
using Application.Stream.Commands;

namespace Application.Interfaces.Common
{
    public interface IBotServiceClient
    {
        Task<HttpResponseMessage> InviteBotAsync(InviteBot.InviteBotCommand command);

        Task<HttpResponseMessage> RemoveBotAsync(string callGraphId);

        Task<DoStartInjection.DoStartInjectionCommandResponse> StartInjectionAsync(DoStartInjection.DoStartInjectionCommand command);

        Task<DoStopInjection.DoStopInjectionCommandResponse> StoptInjectionAsync(DoStopInjection.DoStopInjectionCommand command);

        Task<DoStartExtraction.DoStartExtractionCommandResponse> StartExtractionAsync(DoStartExtraction.DoStartExtractionCommand command);

        Task<DoStopExtraction.DoStopExtractionCommandResponse> StopExtractionAsync(DoStopExtraction.DoStopExtractionCommand command);

        Task<HttpResponseMessage> MuteBotAsync();

        Task<HttpResponseMessage> UnmuteBotAsync();

        void SetBaseUrl(string baseUrl);
    }
}