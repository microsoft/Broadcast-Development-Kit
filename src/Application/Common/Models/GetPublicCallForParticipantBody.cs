using Domain.Enums;

namespace Application.Common.Models
{
    public class GetPublicCallForParticipantBody
    {
        public ResourceType Type { get; set; }
        public string ParticipantAadId { get; set; }
    }
}
