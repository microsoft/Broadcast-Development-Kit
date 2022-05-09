// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Models.Api;
using Application.Service.Commands;
using Application.Stream.Commands;

namespace Application.Interfaces.Common
{
    public interface IBotServiceClient
    {
        Task<HttpResponseMessage> InviteBotAsync(DoInviteBot.DoInviteBotCommand command);

        Task<HttpResponseMessage> RemoveBotAsync(string callGraphId);

        Task<DoStartInjection.DoStartInjectionCommandResponse> StartInjectionAsync(DoStartInjection.DoStartInjectionCommand command);

        Task<DoStopInjection.DoStopInjectionCommandResponse> StoptInjectionAsync(DoStopInjection.DoStopInjectionCommand command);

        Task<DoHideInjection.DoHideInjectionCommandResponse> HideInjectionAsync(DoHideInjection.DoHideInjectionCommand command);

        Task<DoDisplayInjection.DoDisplayInjectionCommandResponse> DisplayInjectionAsync(DoDisplayInjection.DoDisplayInjectionCommand command);

        Task<DoStartExtraction.DoStartExtractionCommandResponse> StartExtractionAsync(DoStartExtraction.DoStartExtractionCommand command);

        Task<DoStopExtraction.DoStopExtractionCommandResponse> StopExtractionAsync(DoStopExtraction.DoStopExtractionCommand command);

        Task<HttpResponseMessage> MuteBotAsync(string callId);

        Task<HttpResponseMessage> UnmuteBotAsync(string callId);

        Task<HttpResponseMessage> SetInjectionVolumeAsync(string callId, SetInjectionVolumeRequest setInjectionVolumeRequest);

        void SetBaseUrl(string baseUrl);
    }
}