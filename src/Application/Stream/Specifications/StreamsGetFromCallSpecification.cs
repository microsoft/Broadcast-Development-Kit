// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Ardalis.Specification;

namespace Application.Stream.Specifications
{
    public class StreamsGetFromCallSpecification : Specification<Domain.Entities.Stream>
    {
        public StreamsGetFromCallSpecification(string callId)
        {
            Query.Where(x => x.CallId == callId);
        }
    }
}