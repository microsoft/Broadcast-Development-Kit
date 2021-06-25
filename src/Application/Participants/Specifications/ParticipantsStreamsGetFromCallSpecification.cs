using Ardalis.Specification;

namespace Application.Participants.Specifications
{
    public class ParticipantsStreamsGetFromCallSpecification : Specification<Domain.Entities.ParticipantStream>
    {
        public ParticipantsStreamsGetFromCallSpecification(string callId, bool archived)
        {
            if (archived)
            {
                Query.Where(x =>
                x.CallId == callId);
            }
            else
            {
                Query.Where(x =>
               x.CallId == callId && x.LeftAt == null);
            }
        }
    }
}
