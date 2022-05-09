// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Application.Service.Specifications;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Extensions.Logging;

namespace Application.Service.Commands
{
    public class UnregisterService
    {
        public class UnregisterServiceCommand : IRequest<UnregisterServiceCommandResponse>
        {
            public string VirtualMachineName { get; set; }
        }

        public class UnregisterServiceCommandResponse
        {
            public string Id { get; set; }
        }

        public class UnregisterServiceCommandValidator : AbstractValidator<UnregisterServiceCommand>
        {
            public UnregisterServiceCommandValidator()
            {
                RuleFor(x => x.VirtualMachineName)
                    .NotEmpty();
            }
        }

        public class UnregisterServiceCommandHandler : IRequestHandler<UnregisterServiceCommand, UnregisterServiceCommandResponse>
        {
            private readonly IServiceRepository _serviceRepository;
            private readonly ILogger<UnregisterServiceCommandHandler> _logger;

            public UnregisterServiceCommandHandler(
                IServiceRepository serviceRepository,
                ILogger<UnregisterServiceCommandHandler> logger)
            {
                _serviceRepository = serviceRepository ?? throw new System.ArgumentNullException(nameof(serviceRepository));
                _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            }

            public async Task<UnregisterServiceCommandResponse> Handle(UnregisterServiceCommand request, CancellationToken cancellationToken)
            {
                var response = new UnregisterServiceCommandResponse();

                var specification = new ServiceGetByVirtualMachineNameSpecification(request.VirtualMachineName);

                var services = await _serviceRepository.GetItemsAsync(specification);

                var service = services.FirstOrDefault();

                if (service == null)
                {
                    _logger.LogError("[Bot Service API] There is no service configured with virtual machine {virtualMachineName} registered", request.VirtualMachineName);
                    throw new EntityNotFoundException($"[Bot Service API]There is no service configured with virtual machine {request.VirtualMachineName} registered");
                }

                service.CallId = null;
                service.State = Domain.Enums.ServiceState.Unavailable;

                /*
                 * NOTE:
                 * This command is executed when the bot service (windows service) is stopped.
                 * Because sometimes the bot's virtual machine is shutted down from Azure Portal,
                 * and the Azure function that updates the state of the infrastructure (VM) is
                 * not executed, we force the state update from this command.
                 *
                 * Most of the times, the state will be upadated by the Azure function triggered by
                 * event grid.
                 */

                service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Deprovisioned;
                service.Infrastructure.ProvisioningDetails.Message = $"Deprovisioned service {service.Name}.";
                service.Infrastructure.PowerState = PowerState.Deallocated.Value;

                await _serviceRepository.UpdateItemAsync(service.Id, service);

                response.Id = service.Id;

                return response;
            }
        }
    }
}
