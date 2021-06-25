using Ardalis.Specification;
using Domain.Enums;

namespace Application.Call.Specifications
{
    public class CallGetActiveSpecification : Specification<Domain.Entities.Call>
    {
        public CallGetActiveSpecification()
        {
            Query.Where(x => x.State == CallState.Established)
                .OrderByDescending(x => x.CreatedAt);
        }
    }
}
