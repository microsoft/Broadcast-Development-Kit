using Ardalis.Specification;
using Domain.Enums;

namespace Application.Stream.Specifications
{
    public class StreamGetActiveFromCallSpecification : Specification<Domain.Entities.Stream>
    {
        public StreamGetActiveFromCallSpecification(string callId)
        {
            Query
                .Where(x => x.CallId == callId && (x.State == StreamState.Starting || x.State == StreamState.Started))
                .OrderByDescending(x => x.StartingAt);
        }
    }
}
