// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Constants;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using static Application.Service.Commands.StartServiceInfrastructure;

namespace Application.Service.Commands
{
    public class StartingServiceInfrastructure
    {
        public class StartingServiceInfrastructureCommand : IRequest<StartingServiceInfrastructureCommandResponse>
        {
            public string ServiceId { get; set; }
        }

        public class StartingServiceInfrastructureCommandResponse
        {
            public string Id { get; set; }

            public ServiceModel Resource { get; set; }
        }

        public class StartingServiceInfrastructureCommandHandler : IRequestHandler<StartingServiceInfrastructureCommand, StartingServiceInfrastructureCommandResponse>
        {
            private readonly IServiceRepository _serviceRepository;
            private readonly IAzStorageHandler _storageHandler;
            private readonly IMapper _mapper;

            public StartingServiceInfrastructureCommandHandler(
                IServiceRepository serviceRepository,
                IAzStorageHandler storageHandler,
                IMapper mapper)
            {
                _serviceRepository = serviceRepository;
                _storageHandler = storageHandler;
                _mapper = mapper;
            }

            public async Task<StartingServiceInfrastructureCommandResponse> Handle(StartingServiceInfrastructureCommand request, CancellationToken cancellationToken)
            {
                var response = new StartingServiceInfrastructureCommandResponse();
                /* TODO: Change this.
                  NOTE: The Management Portal does not have the feature to select the service before initializing the call.
                  The following code is temporary, if the service Id is not specified, we use a harcoded ID to retrieve the service.
                */

                var serviceId = string.IsNullOrEmpty(request.ServiceId) ? Constants.EnvironmentDefaults.ServiceId : request.ServiceId;
                var service = await _serviceRepository.GetItemAsync(serviceId);

                if (service == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Service), serviceId);
                }

                // TODO: Review
                service.State = ServiceState.Unavailable;
                service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Provisioning;
                service.Infrastructure.ProvisioningDetails.Message = $"Provisioning service {service.Name}.";
                await _serviceRepository.UpdateItemAsync(service.Id, service);

                var startVirtualMachineCommand = new StartServiceInfrastructureCommand
                {
                    Id = service.Id,
                };

                await _storageHandler.AddQueueMessageAsync(Constants.AzureQueueNames.StartVirtualMachineQueue, startVirtualMachineCommand);

                response.Id = service.Id;
                response.Resource = _mapper.Map<ServiceModel>(service);

                return response;
            }
        }
    }
}
