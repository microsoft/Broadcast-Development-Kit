using Domain.Enums;

namespace Application.Common.Models
{
    public class SrtStreamExtractionBody : StartStreamExtractionBody
    {
        public int Latency { get; set; }

        public SrtMode Mode { get; set; }
    }
}
