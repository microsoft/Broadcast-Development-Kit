using Domain.Enums;

namespace Application.Common.Models
{
    public class StopStreamExtractionBody
    {
        public string CallId { get; set; }

        public string ParticipantId { get; set; }

        public string ParticipantGraphId { get; set; }

        public ResourceType ResourceType { get; set; }
    }
}
