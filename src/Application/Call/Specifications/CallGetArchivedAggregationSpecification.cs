// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Ardalis.Specification;
using Domain.Enums;

namespace Application.Call.Specifications
{
    public class CallGetArchivedAggregationSpecification : Specification<Domain.Entities.Call>
    {
        public CallGetArchivedAggregationSpecification()
        {
            Query.Where(x => x.State == CallState.Terminated)
                .OrderByDescending(x => x.CreatedAt);
        }
    }
}
