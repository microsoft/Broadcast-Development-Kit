using Domain.Enums;
using Application.Common.Converter;
using Newtonsoft.Json;

namespace Application.Common.Models
{
    [JsonConverter(typeof(StartStreamExtractionBodyConverter))]
    public class StartStreamExtractionBody
    {
        public Protocol Protocol { get; set; }
        public ResourceType ResourceType { get; set; }
        public string CallId { get; set; }
        public string ParticipantId { get; set; }
        public string ParticipantGraphId { get; set; }
        public string StreamUrl { get; set; }
        public string StreamKey { get; set; }
        public bool TimeOverlay { get; set; } = true;
        public SupportedAudioFormat AudioFormat { get; set; } = SupportedAudioFormat.AAC_44100;
    }
}
