using System.Threading.Tasks;
using Application.Call.Queries;
using Application.Common.Models;
using Application.Participants.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParticipantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("photo/{participantAadId}")]
        public async Task<IActionResult> GetPhoto([FromRoute] GetParticipantPhoto.GetParticipantPhotoQuery query)
        {
            var response = await _mediator.Send(query);

            return new FileStreamResult(response.Photo, "image/jpeg");
        }
        
        // This endpoint can be used by people invited to the meeting from outside this tenant, to check the status of their stream (if we are extracting their camera or not) 
        // and get any public context information that might be added to this call.
        // As this is a public endpoint we should not return any private information of the participant here.
        [AllowAnonymous]
        [HttpPost]
        [Route("by-meeting-id/{meetingId}")]
        public async Task<ActionResult<PublicCallModelForParticipant>> GetByMeetingIdAsync([FromRoute] string meetingId, [FromBody] GetPublicCallForParticipantBody body)
        {
            var query = new GetPublicCallForParticipantByMeetingId.GetPublicCallForParticipantByMeetingIdQuery
            { 
                ParticipantAadId = body.ParticipantAadId,
                ResourceType = body.Type,
                MeetingId = meetingId
            };

            var response = await _mediator.Send(query);

            return Ok(response.CallModelForParticipant);
        }
    }
}
