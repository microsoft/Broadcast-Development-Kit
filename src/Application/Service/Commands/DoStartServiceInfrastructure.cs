// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Ardalis.Result;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.Azure.Management.Compute.Fluent;

namespace Application.Service.Commands
{
    public class DoStartServiceInfrastructure
    {
        public class DoStartServiceInfrastructureCommand : IRequest<DoStartServiceInfrastructureCommandResponse>
        {
            public string Id { get; set; }
        }

        public class DoStartServiceInfrastructureCommandResponse
        {
            public string Id { get; set; }

            public ServiceModel Resource { get; set; }
        }

        public class DoStartServiceInfrastructureCommandHandler : IRequestHandler<DoStartServiceInfrastructureCommand, DoStartServiceInfrastructureCommandResponse>
        {
            private readonly IAzVirtualMachineService _azureVirtualMachineService;
            private readonly IServiceRepository _serviceRepository;
            private readonly IHostEnvironment _environment;
            private readonly IMapper _mapper;

            public DoStartServiceInfrastructureCommandHandler(
                IAzVirtualMachineService azureVirtualMachineService,
                IServiceRepository serviceRepository,
                IHostEnvironment environment,
                IMapper mapper)
            {
                _azureVirtualMachineService = azureVirtualMachineService ?? throw new System.ArgumentNullException(nameof(azureVirtualMachineService));
                _serviceRepository = serviceRepository ?? throw new System.ArgumentNullException(nameof(serviceRepository));
                _environment = environment ?? throw new System.ArgumentNullException(nameof(environment));
                _mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            }

            public async Task<DoStartServiceInfrastructureCommandResponse> Handle(DoStartServiceInfrastructureCommand request, CancellationToken cancellationToken)
            {
                var response = new DoStartServiceInfrastructureCommandResponse();

                var service = await _serviceRepository.GetItemAsync(request.Id);
                if (service == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Service), request.Id);
                }

                // TODO: Review
                if (_environment.IsLocal())
                {
                    service.State = ServiceState.Available;
                    service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Provisioned;
                    service.Infrastructure.ProvisioningDetails.Message = $"Provisioned service {service.Name}.";
                    service.Infrastructure.PowerState = PowerState.Running.Value;
                }
                else
                {
                    var result = await _azureVirtualMachineService.StartAsync(service.Infrastructure.Id);

                    // At this point, the VM might be up and running and the bot service might have already register itself as available in the database.
                    // Therefore, we need to retrieve the latest entity of the VM again.
                    // TODO: Improve this to react to collisions using ETag validation
                    service = await _serviceRepository.GetItemAsync(request.Id);
                    ProcessResult(service, result);
                }

                await _serviceRepository.UpdateItemAsync(service.Id, service);

                response.Id = service.Id;
                response.Resource = _mapper.Map<ServiceModel>(service);
                return response;
            }

            private static void ProcessResult(Domain.Entities.Service service, Result<IVirtualMachine> result)
            {
                switch (result.Status)
                {
                    case ResultStatus.Ok:
                        service.Infrastructure.PowerState = result.Value.PowerState.Value;
                        service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Provisioned;
                        service.Infrastructure.ProvisioningDetails.Message = $"Service {service.Name} provisioned.";
                        break;
                    case ResultStatus.Error:
                        StringBuilder sb = new StringBuilder();
                        foreach (var error in result.Errors)
                        {
                            sb.AppendLine(error);
                        }

                        service.Infrastructure.PowerState = PowerState.Unknown.Value;
                        service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Error;
                        service.Infrastructure.ProvisioningDetails.Message = sb.ToString();
                        break;
                    case ResultStatus.Invalid:
                        service.Infrastructure.PowerState = result.ValidationErrors.Count > 0 ? result.ValidationErrors[0].Identifier : string.Empty;
                        service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Error;
                        service.Infrastructure.ProvisioningDetails.Message = result.ValidationErrors.Count > 0 ? result.ValidationErrors[0].ErrorMessage : string.Empty;
                        break;
                    case ResultStatus.NotFound:
                        service.Infrastructure.PowerState = PowerState.Unknown.Value;
                        service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Error;
                        service.Infrastructure.ProvisioningDetails.Message = $"The Virtual Machine with ResourceId: {service.Infrastructure.Id} was not found.";
                        break;
                    default:
                        service.Infrastructure.PowerState = PowerState.Unknown.Value;
                        service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Error;
                        service.Infrastructure.ProvisioningDetails.Message = $"An error ocurred while trying to start Virtual Machine. ResourceId : {service.Infrastructure.Id}";
                        break;
                }
            }
        }
    }
}
