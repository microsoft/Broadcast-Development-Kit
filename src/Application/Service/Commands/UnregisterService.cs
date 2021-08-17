// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Application.Service.Specifications;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
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

                var entity = services.FirstOrDefault();

                if (entity == null)
                {
                    _logger.LogError("[Bot Service API] There is no service configured with virtual machine {virtualMachineName} registered", request.VirtualMachineName);
                    throw new EntityNotFoundException($"[Bot Service API]There is no service configured with virtual machine {request.VirtualMachineName} registered");
                }

                entity.CallId = null;
                entity.State = Domain.Enums.ServiceState.Unavailable;

                await _serviceRepository.UpdateItemAsync(entity.Id, entity);

                response.Id = entity.Id;

                return response;
            }
        }
    }
}
