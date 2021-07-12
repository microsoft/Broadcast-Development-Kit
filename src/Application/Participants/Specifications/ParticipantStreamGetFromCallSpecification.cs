// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Ardalis.Specification;

namespace Application.Participants.Specifications
{
    public class ParticipantStreamGetFromCallSpecification : Specification<Domain.Entities.ParticipantStream>
    {
        public ParticipantStreamGetFromCallSpecification(string callId, string participantId)
        {
            Query.Where(x =>
                x.CallId == callId && x.ParticipantGraphId == participantId);
        }
    }
}
