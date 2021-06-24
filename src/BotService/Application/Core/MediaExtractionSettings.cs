using Microsoft.Skype.Bots.Media;

namespace BotService.Application.Core
{
    public class MediaExtractionSettings
    {
        public MediaType MediaType { get; set; }
        public uint MediaSourceId { get; set; }
        public VideoResolution VideoResolution { get; set; }
        public ProtocolSettings ProtocolSettings { get; set; }
    }
}