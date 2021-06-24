using Domain.Enums;
using Microsoft.Skype.Bots.Media;

namespace BotService.Application.Core
{
    public interface IMediaExtractor
    {
        Protocol Protocol { get; }

        IVideoSocket VideoSocket { get; }

        void Start(MediaExtractionSettings mediaStreamSettings);

        void Stop();
    }
}
