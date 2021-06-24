using Ardalis.Specification;
using Domain.Enums;

namespace Application.Call.Specifications
{
    class CallGetArchivedSpecification : Specification<Domain.Entities.Call>
    {
        public CallGetArchivedSpecification(int pageNumber = 0,
                                             int pageSize = 50)
        {
            Query.Where(x => x.State == CallState.Terminated)
                .OrderByDescending(x => x.CreatedAt);

            // Pagination
            if (pageSize != -1) //Display all entries and disable pagination 
            {
                Query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            }
        }
    }
}
