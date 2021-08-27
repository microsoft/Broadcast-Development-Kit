// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Application.Service.Events;
using AutoMapper;
using Domain.Constants;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using static Application.Service.Commands.DoStopServiceInfrastructure;

namespace Application.Service.Commands
{
    public class RequestStopServiceInfrastructure
    {
        public class RequestStopServiceInfrastructureCommand : IRequest<RequestStopServiceInfrastructureCommandResponse>
        {
            public string ServiceId { get; set; }
        }

        public class RequestStopServiceInfrastructureCommandResponse
        {
            public string Id { get; set; }

            public ServiceModel Resource { get; set; }
        }

        public class RequestStopServiceInfrastructureCommandHandler : IRequestHandler<RequestStopServiceInfrastructureCommand, RequestStopServiceInfrastructureCommandResponse>
        {
            private readonly IServiceRepository _serviceRepository;
            private readonly IMediator _mediator;
            private readonly IMapper _mapper;

            public RequestStopServiceInfrastructureCommandHandler(
                IServiceRepository serviceRepository,
                IMediator mediator,
                IMapper mapper)
            {
                _serviceRepository = serviceRepository ?? throw new System.ArgumentNullException(nameof(serviceRepository));
                _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
                _mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            }

            public async Task<RequestStopServiceInfrastructureCommandResponse> Handle(RequestStopServiceInfrastructureCommand request, CancellationToken cancellationToken)
            {
                var response = new RequestStopServiceInfrastructureCommandResponse();

                /* TODO: Change this.
                   NOTE: The Management Portal does not have the feature to select the service before initializing the call.
                   The folloiwng code is temporary, if the service Id is not specified, we use a harcoded ID to retrieve the service.
               */

                var serviceId = string.IsNullOrEmpty(request.ServiceId) ? Constants.EnvironmentDefaults.ServiceId : request.ServiceId;
                var service = await _serviceRepository.GetItemAsync(serviceId);

                if (service == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Service), serviceId);
                }

                // TODO: Review
                service.State = ServiceState.Unavailable;
                service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Deprovisioning;
                service.Infrastructure.ProvisioningDetails.Message = $"Deprovisioning service {service.Name}";

                await _serviceRepository.UpdateItemAsync(service.Id, service);

                var stopServiceInfrastructureRequestedEvent = new StopServiceInfrastructureRequested.StopServiceInfrastructureRequestedEvent
                {
                    CallId = service.CallId,
                    ServiceId = service.Id,
                };

                await _mediator.Publish(stopServiceInfrastructureRequestedEvent);

                response.Id = service.Id;
                response.Resource = _mapper.Map<ServiceModel>(service);

                return response;
            }
        }
    }
}
