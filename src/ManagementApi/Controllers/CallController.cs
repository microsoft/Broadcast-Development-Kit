// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Call.Commands;
using Application.Call.Queries;
using Application.Common.Models;
using Application.Stream.Commands;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CallController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/<CallController>
        [HttpPost]
        [Route("initialize-call")]
        public async Task<IActionResult> InviteBot([FromBody] RequestInviteBot.RequestInviteBotCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<CallModel>> GetAsync([FromRoute] string id, [FromQuery] bool archive)
        {
            var query = new GetCall.GetCallQuery
            {
                Id = id,
                Archived = archive,
            };

            var response = await _mediator.Send(query);

            return Ok(response.Call);
        }

        [HttpGet]
        [Route("by-meeting-id/{meetingId}")]
        public async Task<ActionResult<CallModel>> GetByMeetingIdAsync([FromRoute] string meetingId, [FromQuery] bool archive)
        {
            var query = new GetCallByMeetingId.GetCallByMeetingIdQuery
            {
                MeetingId = meetingId,
                Archived = archive,
            };

            var response = await _mediator.Send(query);

            return Ok(response.Call);
        }

        [HttpDelete]
        [Route("{callId}")]
        public async Task<ActionResult> EndCallAsync([FromRoute] string callId)
        {
            var command = new EndCall.EndCallCommand
            {
                CallId = callId,
                ShouldShutDownService = false,
            };
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpGet]
        [Route("active")]
        public async Task<ActionResult<List<CallModel>>> GetActiveCallsAsync()
        {
            var query = new GetActiveCalls.GetActiveCallsQuery();
            var response = await _mediator.Send(query);

            return Ok(response.Calls);
        }

        [HttpGet]
        [Route("archived")]
        public async Task<ActionResult<PagedQueryResult<CallModel>>> GetArchivedCallsAsync([FromQuery] int pageNumber, int pageSize = 10)
        {
            var query = new GetArchivedCalls.GetArchivedCallsQuery
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
            };

            var response = await _mediator.Send(query);

            return Ok(response.Result);
        }

        [HttpPost]
        [Route("{callId}/stream/start-extraction")]
        public async Task<ActionResult> StartExtractionAsync([FromRoute] string callId, [FromBody] StartStreamExtractionBody streamExtraction)
        {
            streamExtraction.CallId = callId;
            var command = new StartingExtraction.StartingExtractionCommand
            {
                Body = streamExtraction,
            };
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("{callId}/stream/stop-extraction")]
        public async Task<ActionResult> StopExtractionAsync([FromRoute] string callId, [FromBody] StopStreamExtractionBody streamExtraction)
        {
            streamExtraction.CallId = callId;
            var command = new StoppingExtraction.StoppingExtractionCommand
            {
                Body = streamExtraction,
            };
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("{callId}/stream/start-injection")]
        public async Task<ActionResult> StartInjectionAsync([FromRoute] string callId, [FromBody] StartStreamInjectionBody streamInjection)
        {
            streamInjection.CallId = callId;

            var command = new StartingInjection.StartingInjectionCommand
            {
                Body = streamInjection,
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("{callId}/stream/{streamId}/stop-injection")]
        public async Task<ActionResult> StopInjectionAsync([FromRoute] string callId, [FromRoute] string streamId)
        {
            var command = new StoppingInjection.StoppingInjectionCommand
            {
                CallId = callId,
                StreamId = streamId,
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("{callId}/mute")]
        public async Task<ActionResult> MuteAsync([FromRoute] string callId)
        {
            var command = new MuteBotFromCall.MuteBotFromCallCommand
            {
                CallId = callId,
            };
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost]
        [Route("{callId}/unmute")]
        public async Task<ActionResult> UnmuteAsync([FromRoute] string callId)
        {
            var command = new UnmuteBotFromCall.UnmuteBotFromCallCommand
            {
                CallId = callId,
            };
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost]
        [Route("{callId}/generate-stream-key")]
        public async Task<ActionResult> GenerateStreamKeyAsync([FromRoute] string callId)
        {
            var command = new GenerateStreamKey.GenerateStreamKeyCommand
            {
                CallId = callId,
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost]
        [Route("{callId}/context/{privacyLevel}")]
        public async Task<ActionResult> PostContextAsync([FromRoute] string callId, [FromRoute] ContextPrivacy privacyLevel, [FromBody] Dictionary<string, string> values)
        {
            var command = new SetCallContext.SetCallContextCommand
            {
                CallId = callId,
                PrivacyLevel = privacyLevel,
                Values = values,
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete]
        [Route("{callId}/context/{privacyLevel}")]
        public async Task<ActionResult> ClearContextAsync([FromRoute] string callId, [FromRoute] ContextPrivacy privacyLevel)
        {
            var command = new DeleteCallContext.DeleteCallContextCommand
            {
                CallId = callId,
                PrivacyLevel = privacyLevel,
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}
