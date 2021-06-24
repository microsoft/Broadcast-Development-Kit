using Application.Common.Models;
using Ardalis.Result;
using System.Net.Http;
using System.Threading.Tasks;
using static Application.Service.Commands.InviteBot;

namespace Application.Interfaces.Common
{
    public interface IBot
    {
        string Id { get;  }

        string VirtualMachineName { get; set; }

        Task InviteBotAsync(InviteBotCommand command);

        Task ProcessNotificationAsync(HttpRequestMessage request);

        Task RemoveBotAsync(string callGraphId);

        void StartInjection(StartStreamInjectionBody startStreamInjectionBody);

        void StopInjection();

        StartStreamExtractionResponse StartExtraction(StartStreamExtractionBody streamBody);

        void StopExtraction(StopStreamExtractionBody streamBody);

        Task MuteBotAsync();

        Task UnmuteBotAsync();
        
        Task RegisterServiceAsync(string virtualMachineName);
    }
}

