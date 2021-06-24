using Domain.Enums;

namespace BotService.Application.Core
{
    public abstract class ProtocolSettings
    {
        public Protocol Type { get; set; }

        public bool TimeOverlay { get; set; } = true;

        public SupportedAudioFormat AudioFormat { get; set; } = SupportedAudioFormat.AAC_44100;
    }
}
