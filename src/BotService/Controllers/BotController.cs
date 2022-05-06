﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading.Tasks;
using Application.Common.Config;
using Application.Common.Models.Api;
using Application.Service.Commands;
using Application.Stream;
using Application.Stream.Commands;
using BotService.Infrastructure.Common.Logging;
using Infrastructure.Core.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Service.Commands.ProcessNotification;

namespace BotService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SampleObserver _observer;
        private readonly PipelineBusObserver _pipelineBusObserver;

        public BotController(
            IMediator mediator,
            SampleObserver observer,
            PipelineBusObserver pipelineBusObserver)
        {
            _mediator = mediator;
            _observer = observer;
            _pipelineBusObserver = pipelineBusObserver;
        }

        // POST api/<BotController>
        [HttpPost]
        [Route("invite")]
        public async Task<IActionResult> InviteBotAsync([FromBody] DoInviteBot.DoInviteBotCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpDelete]
        [Route("call/{graphCallId}")]
        public async Task<ActionResult> EndCallAsync([FromRoute] string graphCallId)
        {
            var command = new DoEndCall.DoEndCallCommand
            {
                GraphCallId = graphCallId,
            };

            await _mediator.Send(command);

            return Ok();
        }

        [HttpPost]
        [Route("call/{callId}/mute")]
        public async Task<ActionResult> MuteAsync([FromRoute] string callId)
        {
            var command = new DoMuteBot.DoMuteBotCommand
            {
                CallId = callId,
            };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost]
        [Route("call/{callId}/unmute")]
        public async Task<ActionResult> UnmuteAsync([FromRoute] string callId)
        {
            var command = new DoUnmuteBot.DoUnmuteBotCommand
            {
                CallId = callId,
            };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost]
        [Route("call/{callId}/stream/start-injection")]
        public async Task<ActionResult> StartInjectionAsync([FromRoute] string callId, [FromBody] DoStartInjection.DoStartInjectionCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("call/{callId}/stream/{streamId}/stop-injection")]
        public async Task<ActionResult> StopInjectionAsync([FromRoute] string callId, [FromRoute] string streamId)
        {
            var command = new DoStopInjection.DoStopInjectionCommand
            {
                CallId = callId,
                StreamId = streamId,
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("call/{callId}/injection/display")]
        public async Task<ActionResult> DisplayInjectionAsync([FromRoute] string callId)
        {
            var command = new DoDisplayInjection.DoDisplayInjectionCommand
            {
                CallId = callId,
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("call/{callId}/injection/hide")]
        public async Task<ActionResult> HideInjectionAsync([FromRoute] string callId)
        {
            var command = new DoHideInjection.DoHideInjectionCommand
            {
                CallId = callId,
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("call/{callId}/stream/start-extraction")]
        public async Task<ActionResult> StartExtractionAsync([FromRoute] string callId, [FromBody] DoStartExtraction.DoStartExtractionCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("call/{callId}/stream/stop-extraction")]
        public async Task<ActionResult> StopExtractionAsync([FromRoute] string callId, [FromBody] DoStopExtraction.DoStopExtractionCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("validate-stream-key")]
        [AllowAnonymous]
        public async Task<ActionResult> ValidateStreamKeyAsync([FromForm] string callId, [FromForm] string name)
        {
            // NGINX sends the stream key value in the 'name' form parameter
            var command = new ValidateStreamKey.ValidateStreamKeyCommand
            {
                CallId = callId,
                StreamKey = name,
            };

            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPost]
        [Route("call/{callId}/injection/set-volume")]
        public async Task<IActionResult> SetInjectionVolumeAsync([FromRoute] string callId, SetInjectionVolumeRequest streamVolume)
        {
            var command = new DoSetInjectionVolume.DoSetInjectionVolumeCommand
            {
                Format = streamVolume.Format,
                Value = streamVolume.Value,
                CallId = callId,
            };

            await _mediator.Send(command);

            return NoContent();
        }

        // POST api/bot/calling - Graph Notification
        // The validity of this request is checked down the line in the AuthenticationProvider.
        [HttpPost]
        [AllowAnonymous]
        [Route(HttpRouteConstants.OnIncomingRequestRoute)]
        public async Task<IActionResult> OnIncomingRequestAsync()
        {
            var command = new ProcessNotificationCommand
            {
                HttpRequestMessage = Request.CreateRequestMessage(),
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        /// <summary>
        /// Get logs from the service.
        /// </summary>
        /// <param name="skip">Skip specified lines.</param>
        /// <param name="take">Take specified lines.</param>
        /// <returns>Complete logs from the service.</returns>
        [HttpGet]
        [Route(HttpRouteConstants.Logs)]
        public IActionResult OnGetLogs(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 1000)
        {
            AddRefreshHeader(3);
            return Content(
                    _observer.GetLogs(skip, take),
                    System.Net.Mime.MediaTypeNames.Text.Plain,
                    System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Get logs from the service.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="skip">Skip specified lines.</param>
        /// <param name="take">Take specified lines.</param>
        /// <returns>
        /// Complete logs from the service.
        /// </returns>
        [HttpGet]
        [Route(HttpRouteConstants.Logs + "{filter}")]
        public IActionResult OnGetLogs(
            string filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 1000)
        {
            AddRefreshHeader(3);
            return Content(
                _observer.GetLogs(filter, skip, take),
                System.Net.Mime.MediaTypeNames.Text.Plain,
                System.Text.Encoding.UTF8);
        }

        [HttpGet]
        [Route("stream/" + HttpRouteConstants.Logs)]
        public IActionResult OnGetStreamsLogs(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 1000)
        {
            AddRefreshHeader(3);
            return Content(
                    _pipelineBusObserver.GetLogs(skip, take),
                    System.Net.Mime.MediaTypeNames.Text.Plain,
                    System.Text.Encoding.UTF8);
        }

        [HttpGet]
        [Route("stream/" + HttpRouteConstants.Logs + "{filter}")]
        public IActionResult OnGetStreamLogs(
            string filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 1000)
        {
            AddRefreshHeader(3);
            return Content(
                _pipelineBusObserver.GetLogs(filter, skip, take),
                System.Net.Mime.MediaTypeNames.Text.Plain,
                System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Add refresh headers for browsers to download content.
        /// </summary>
        /// <param name="seconds">Refresh rate.</param>
        private void AddRefreshHeader(int seconds)
        {
            Response.Headers.Add("Cache-Control", "private,must-revalidate,post-check=1,pre-check=2,no-cache");
            Response.Headers.Add("Refresh", seconds.ToString());
        }
    }
}