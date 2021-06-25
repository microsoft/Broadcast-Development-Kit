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

                var entity = services.FirstOrDefault();

                if (entity == null)
                {
                    _logger.LogError("[Bot Service API] There is no service configured with virtual machine {virtualMachineName} registered", request.VirtualMachineName);
                    throw new EntityNotFoundException($"[Bot Service API]There is no service configured with virtual machine {request.VirtualMachineName} registered");
                }

                entity.CallId = null;
                entity.State = Domain.Enums.ServiceState.Available;

                await _serviceRepository.UpdateItemAsync(entity.Id, entity);

                response.Id = entity.Id;

                return response;
            }
        }
    }
}
