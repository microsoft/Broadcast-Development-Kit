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
    public class StopServiceInfrastructure
    {
        public class StopServiceInfrastructureCommand : IRequest<StopServiceInfrastructureCommandResponse>
        {
            public string Id { get; set; }
        }

        public class StopServiceInfrastructureCommandResponse
        {
            public string Id { get; set; }

            public ServiceModel Resource { get; set; }
        }

        public class StopServiceInfrastructureCommandHandler : IRequestHandler<StopServiceInfrastructureCommand, StopServiceInfrastructureCommandResponse>
        {
            private readonly IAzVirtualMachineService _azureVirtualMachineService;
            private readonly IServiceRepository _serviceRepository;
            private readonly IHostEnvironment _environment;
            private readonly IMapper _mapper;

            public StopServiceInfrastructureCommandHandler(
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

            public async Task<StopServiceInfrastructureCommandResponse> Handle(StopServiceInfrastructureCommand request, CancellationToken cancellationToken)
            {
                var response = new StopServiceInfrastructureCommandResponse();

                var service = await _serviceRepository.GetItemAsync(request.Id);
                if (service == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Service), request.Id);
                }

                // TODO: Review
                if (_environment.IsLocal())
                {
                    service.State = ServiceState.Stopped;
                    service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Deprovisioned;
                    service.Infrastructure.ProvisioningDetails.Message = $"Deprovisioned service {service.Name}.";
                    service.Infrastructure.PowerState = PowerState.Deallocated.Value;
                }
                else
                {
                    var result = await _azureVirtualMachineService.StopAsync(service.Infrastructure.Id);

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
                        service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Deprovisioned;
                        service.Infrastructure.ProvisioningDetails.Message = $"Service {service.Name} deprovisioned.";
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
