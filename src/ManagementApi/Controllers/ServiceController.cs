// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading.Tasks;
using Application.Service.Commands;
using Application.Service.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("{serviceId}/start")]
        public async Task<IActionResult> Start([FromRoute] string serviceId)
        {
            var command = new StartingServiceInfrastructure.StartingServiceInfrastructureCommand
            {
                ServiceId = serviceId,
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(AddService.AddServiceCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost]
        [Route("{serviceId}/stop")]
        public async Task<IActionResult> Stop([FromRoute] string serviceId)
        {
            var command = new StoppingServiceInfrastructure.StoppingServiceInfrastructureCommand
            {
                ServiceId = serviceId,
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpGet]
        [Route("{serviceId}/state")]
        public async Task<IActionResult> State([FromRoute] string serviceId)
        {
            var query = new GetService.GetServiceQuery
            {
                ServiceId = serviceId,
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}
