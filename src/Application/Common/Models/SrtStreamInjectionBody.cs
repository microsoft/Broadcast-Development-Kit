using Domain.Enums;

namespace Application.Common.Models
{
    public class SrtStreamInjectionBody: StartStreamInjectionBody
    {
        public int Latency { get; set; }
        public SrtMode Mode { get; set; }
    }
}
