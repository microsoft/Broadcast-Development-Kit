namespace BotService.Application.Core
{
    public interface ISwitchingMediaExtractor : IMediaExtractor
    {
        void SwitchMediaSourceSafely(uint mediaSourceId);

        void SwitchMediaSourceForcefully(uint mediaSourceId);
    }
}
