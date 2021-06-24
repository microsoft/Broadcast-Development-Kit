using Ardalis.Specification;

namespace Application.Service.Specifications
{
    public class ServiceGetAllSpecifications : Specification<Domain.Entities.Service>
    {
        public ServiceGetAllSpecifications()
        {
            Query.OrderByDescending(x => x.CreatedAt);
        }
    }
}
