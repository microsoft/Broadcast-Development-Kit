using System;
using Ardalis.Specification;
using Domain.Enums;

namespace Application.Call.Specifications
{
    public class CallGetByMeetingIdSpecification : Specification<Domain.Entities.Call>
    {
        public CallGetByMeetingIdSpecification(string meetingId)
        {
            var today = DateTime.Today.Date;

            Query.Where(x => x.MeetingId == meetingId
                && (x.State == CallState.Establishing || x.State == CallState.Established)
                && x.CreatedAt > today).OrderByDescending(x => x.CreatedAt);
        }
    }
}
