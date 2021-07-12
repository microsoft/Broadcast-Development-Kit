// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Ardalis.Specification;
using Domain.Enums;

namespace Application.Call.Specifications
{
    public class CallGetArchivedSpecification : Specification<Domain.Entities.Call>
    {
        public CallGetArchivedSpecification(int pageNumber = 0, int pageSize = 50)
        {
            Query.Where(x => x.State == CallState.Terminated)
                .OrderByDescending(x => x.CreatedAt);

            // Pagination - Display all entries and disable pagination
            if (pageSize != -1)
            {
                Query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            }
        }
    }
}
