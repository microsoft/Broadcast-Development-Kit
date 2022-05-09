// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Ardalis.Specification;
using Domain.Enums;

namespace Application.Stream.Specifications
{
    public class StreamGetActiveFromCallSpecification : Specification<Domain.Entities.Stream>
    {
        public StreamGetActiveFromCallSpecification(string callId)
        {
            Query
                .Where(x => x.CallId == callId && (x.State == StreamState.Starting || x.State == StreamState.Ready || x.State == StreamState.Receiving || x.State == StreamState.NotReceiving))
                .OrderByDescending(x => x.StartingAt);
        }
    }
}
