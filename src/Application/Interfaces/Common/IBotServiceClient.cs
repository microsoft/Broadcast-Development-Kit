using Application.Service.Commands;
using Application.Service.Queries;
using Application.Stream.Commands;
using System.Net.Http;
using System.Threading.Tasks;

namespace Application.Interfaces.Common
{
    public interface IBotServiceClient
    {
        Task<HttpResponseMessage> InviteBotAsync(InviteBot.InviteBotCommand command);

        Task<HttpResponseMessage> RemoveBotAsync(string callGraphId);

        Task<StartInjection.StartInjectionCommandResponse> StartInjectionAsync(StartInjection.StartInjectionCommand command);

        Task<StopInjection.StopInjectionCommandResponse> StoptInjectionAsync(StopInjection.StopInjectionCommand command);

        Task<StartExtraction.StartExtractionCommandResponse> StartExtractionAsync(StartExtraction.StartExtractionCommand command);

        Task<StopExtraction.StopExtractionCommandResponse> StopExtractionAsync(StopExtraction.StopExtractionCommand command);

        Task<HttpResponseMessage> MuteBotAsync();

        Task<HttpResponseMessage> UnmuteBotAsync();
        
        void SetBaseUrl(string baseUrl);
    }
}