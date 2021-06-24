using Microsoft.Skype.Bots.Media;

namespace BotService.Application.Core
{
    public interface IMediaHandlerFactory
    {
        IMediaExtractor CreateExtractor(IVideoSocket videoSocket, IAudioSocket audioSocket);

        ISwitchingMediaExtractor CreateSwitchingExtractor(IVideoSocket videoSocket, IMediaSocketPool mediaSocketPool, IAudioSocket audioSocket);

        IMediaInjector CreateInjector(IVideoSocket videoSocket, IAudioSocket audioSocket);
    }
}
