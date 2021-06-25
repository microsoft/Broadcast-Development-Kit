using Ardalis.Specification;

namespace Application.Service.Specifications
{
    public class ServiceGetByCallIdSpecification : Specification<Domain.Entities.Service>
    {
        public ServiceGetByCallIdSpecification(string callId)
        {
            Query.Where(x =>
                x.CallId == callId);
        }
    }
}
