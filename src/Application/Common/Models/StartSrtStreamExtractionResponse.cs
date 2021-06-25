using Domain.Enums;

namespace Application.Common.Models
{
    public class StartSrtStreamExtractionResponse : StartStreamExtractionResponse
    {
        public SrtMode Mode { get; set; }

        public string Url { get; set; }

        public string Passphrase { get; set; }

        public int Latency { get; set; }
    }
}
