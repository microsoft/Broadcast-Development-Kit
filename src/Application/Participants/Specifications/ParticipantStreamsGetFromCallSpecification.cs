// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Ardalis.Specification;
using Domain.Enums;
using static Domain.Constants.Constants;

namespace Application.Participants.Specifications
{
    public class ParticipantStreamsGetFromCallSpecification : Specification<Domain.Entities.ParticipantStream>
    {
        public ParticipantStreamsGetFromCallSpecification(string callId, ResourceType type, string participantAadId)
        {
            switch (type)
            {
                case ResourceType.Participant:
                    Query.Where(x => x.CallId == callId && x.Type == type && x.AadId == participantAadId && x.LeftAt == null);
                    break;
                case ResourceType.PrimarySpeaker:
                    Query.Where(x => x.CallId == callId && x.Type == type && x.DisplayName == DefaultParticipantsDisplayNames.PrimarySpeaker && x.LeftAt == null);
                    break;
                case ResourceType.Vbss:
                    Query.Where(x => x.CallId == callId && x.Type == type && x.DisplayName == DefaultParticipantsDisplayNames.ScreenShare && x.LeftAt == null);
                    break;
                default:
                    break;
            }
        }
    }
}
