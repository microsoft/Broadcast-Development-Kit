using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Application.Service.Specifications;
using Domain.Constants;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Service.Commands
{
    public class HandleEventGridServiceInfrastructureEvent
    {
        public class HandleEventGridServiceInfrastructureEventCommand : IRequest
        {
            public string EventType { get; set; }

            public dynamic Data { get; set; }

            public string ServiceInfrastructureId { get; set; }
        }

        public class HandleEventGridServiceInfrastructureEventCommandHandler : IRequestHandler<HandleEventGridServiceInfrastructureEventCommand>
        {
            private readonly IServiceRepository _serviceRepository;
            private readonly ILogger<HandleEventGridServiceInfrastructureEventCommandHandler> _logger;
            private readonly List<string> _operationTypes = new List<string>
            {
                Constants.AzureEventGid.VirtualMachineOperationType.Start,
                Constants.AzureEventGid.VirtualMachineOperationType.Deallocate,
            };

            public HandleEventGridServiceInfrastructureEventCommandHandler(
                IServiceRepository serviceRepository,
                ILogger<HandleEventGridServiceInfrastructureEventCommandHandler> logger)
            {
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public List<string> OperationTypes => _operationTypes;

            public async Task<Unit> Handle(HandleEventGridServiceInfrastructureEventCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    if (request.EventType != Constants.AzureEventGid.EventTypes.ResourceActionSuccessEvent)
                    {
                        _logger.LogInformation("The event type {eventType} is different than the expected", request.EventType);
                        return Unit.Value;
                    }

                    string operation = (request.Data.operationName ?? string.Empty).ToString();
                    if (!_operationTypes.Contains(operation))
                    {
                        string data = request.Data.ToString();
                        _logger.LogInformation(
                            "The event's operation name {name} is not part of the handler's scope.\nData: {data}",
                            operation,
                            data);

                        return Unit.Value;
                    }

                    (string powerState, ProvisioningStateType provisioningState) =
                        operation == Constants.AzureEventGid.VirtualMachineOperationType.Deallocate ?
                        (Microsoft.Azure.Management.Compute.Fluent.PowerState.Deallocated.Value, ProvisioningStateType.Deprovisioned) :
                        (Microsoft.Azure.Management.Compute.Fluent.PowerState.Running.Value, ProvisioningStateType.Provisioned);

                    var virtualMachineSpecification = new ServiceGetByInfrastructureIdSpecification(request.ServiceInfrastructureId);
                    var service = await _serviceRepository.GetFirstItemAsync(virtualMachineSpecification);

                    if (service == null)
                    {
                        _logger.LogError("Service with infrastructure id {infrastructureId} doesn't exist.", request.ServiceInfrastructureId);
                        return Unit.Value;
                    }

                    service.Infrastructure.PowerState = powerState;
                    service.Infrastructure.ProvisioningDetails = new Domain.Entities.Parts.ProvisioningDetails
                    {
                        State = provisioningState,
                    };

                    await _serviceRepository.UpdateItemAsync(service.Id, service);
                    _logger.LogInformation(
                        "Service {serviceName} / Infrastructure {infrastructureName} has been succesfully updated.\n Provisioning State {provisioningState} - Powerstate {powerState}.",
                        service.Name,
                        service.Infrastructure.VirtualMachineName,
                        provisioningState,
                        powerState);

                    return Unit.Value;
                }
                catch (Exception ex)
                {
                    string data = request.Data.ToString();
                    _logger.LogError(ex, "An error ocurred procesing the event data\nEvent data object: {data}", data);
                    return Unit.Value;
                }
            }
        }
    }
}
