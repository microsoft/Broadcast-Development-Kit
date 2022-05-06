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
    public class RegisterService
    {
        public class RegisterServiceCommand : IRequest<RegisterServiceCommandResponse>
        {
            public string VirtualMachineName { get; set; }
        }

        public class RegisterServiceCommandResponse
        {
            public string Id { get; set; }
        }

        public class RegisterServiceCommandValidator : AbstractValidator<RegisterServiceCommand>
        {
            public RegisterServiceCommandValidator()
            {
                RuleFor(x => x.VirtualMachineName)
                    .NotEmpty();
            }
        }

        public class RegisterServiceCommandHandler : IRequestHandler<RegisterServiceCommand, RegisterServiceCommandResponse>
        {
            private readonly IServiceRepository _serviceRepository;
            private readonly ILogger<RegisterServiceCommandHandler> _logger;

            public RegisterServiceCommandHandler(
                IServiceRepository serviceRepository,
                ILogger<RegisterServiceCommandHandler> logger)
            {
                _serviceRepository = serviceRepository ?? throw new System.ArgumentNullException(nameof(serviceRepository));
                _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            }

            public async Task<RegisterServiceCommandResponse> Handle(RegisterServiceCommand request, CancellationToken cancellationToken)
            {
                var response = new RegisterServiceCommandResponse();

                var specification = new ServiceGetByVirtualMachineNameSpecification(request.VirtualMachineName);

                var services = await _serviceRepository.GetItemsAsync(specification);

                var service = services.FirstOrDefault();

                if (service == null)
                {
                    _logger.LogError("[Bot Service API] There is no service configured with virtual machine {virtualMachineName} registered", request.VirtualMachineName);
                    throw new EntityNotFoundException($"[Bot Service API]There is no service configured with virtual machine {request.VirtualMachineName} registered");
                }

                service.CallId = null;
                service.State = ServiceState.Available;

                /*
                 * NOTE:
                 * This command is executed when the bot service starts running.
                 * Because sometimes the bot's virtual machine is turned on from Azure Portal,
                 * and the Azure function that updates the state of the infrastructure (VM) is
                 * not executed, we force the state update from this command.
                 */
                service.Infrastructure.ProvisioningDetails.State = ProvisioningStateType.Provisioned;
                service.Infrastructure.ProvisioningDetails.Message = $"Provisioned service {service.Name}.";
                service.Infrastructure.PowerState = PowerState.Running.Value;

                await _serviceRepository.UpdateItemAsync(service.Id, service);

                response.Id = service.Id;

                return response;
            }
        }
    }
}
