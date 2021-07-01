using Domain.Enums;

namespace Domain.Entities.Parts
{
    public class StreamDetails
    {
        public string StreamUrl { get; set; }

        public string StreamKey { get; set; }

        public int Latency { get; set; }

        public string PreviewUrl { get; set; }

        public Protocol Protocol { get; set; }

        public dynamic Mode { get; set; }

        public bool EnableSsl { get; set; }
    }
}
