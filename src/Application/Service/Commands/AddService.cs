using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Application.Service.Specifications;
using AutoMapper;
using Domain.Constants;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Management.Compute.Fluent;

namespace Application.Service.Commands
{
    public class AddService
    {
        public class AddServiceCommand : IRequest<AddServiceCommandResponse>
        {
            public string FriendlyName { get; set; }
            public string ResourceGroup { get; set; }
            public string SubscriptionId { get; set; }
            public string Name { get; set; }
            public string Dns { get; set; }
            public bool IsDefault { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class AddServiceCommandResponse
        {
            public string Id { get; set; }
            public ServiceModel Resource { get; set; }
        }

        public class AddServiceCommandValidator : AbstractValidator<AddServiceCommand>
        {
            public AddServiceCommandValidator()
            {
                RuleFor(x => x.FriendlyName)
                    .NotEmpty();
                RuleFor(x => x.Name)
                    .NotEmpty();
                RuleFor(x => x.ResourceGroup)
                    .NotEmpty();
                RuleFor(x => x.SubscriptionId)
                    .NotEmpty();
                RuleFor(x => x.Dns)
                    .NotEmpty();
            }
        }

        public class AddServiceCommandHandler : IRequestHandler<AddServiceCommand, AddServiceCommandResponse>
        {
            private readonly IServiceRepository _serviceRepository;
            private readonly IAzVirtualMachineService _virtualMachineService;
            private readonly IHostEnvironment _environment;
            private readonly IMapper _mapper;

            public AddServiceCommandHandler(IServiceRepository serviceRepository,
                IAzVirtualMachineService virtualMachineService,
                IHostEnvironment environment,
                IMapper mapper)
            {
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
                _virtualMachineService = virtualMachineService ?? throw new ArgumentNullException(nameof(virtualMachineService));
                _environment = environment ?? throw new ArgumentNullException(nameof(environment));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<AddServiceCommandResponse> Handle(AddServiceCommand request, CancellationToken cancellationToken)
            {
                var response = new AddServiceCommandResponse();

                IVirtualMachine virtualMachine;
                string virtualMachineName = Constants.EnvironmentDefaults.VirtualMachineName;
                string resourceId = Constants.EnvironmentDefaults.VirtualMachineResourceId;
                string ipAddress = Constants.EnvironmentDefaults.IpAddress;
                string powerState = PowerState.Running.Value;
                var provisioningDetails = new Domain.Entities.ProvisioningDetails
                {
                    State = ProvisioningStateType.Provisioned,
                    Message = string.Empty
                };

                if (!_environment.IsLocal())
                {
                    virtualMachine = await CheckIfResourceExistsInAzure(request);
                    resourceId = virtualMachine.Id;
                    var publicIpAddress = virtualMachine.GetPrimaryPublicIPAddress();
                    ipAddress = publicIpAddress.IPAddress;
                    powerState = virtualMachine.PowerState.Value;
                    virtualMachineName = request.Name;
                    provisioningDetails.State = ProvisioningStateType.Deprovisioned; //The VM must be off before adding the service
                }

                await CheckIfResourceHasBeenRegistered(resourceId);

                var entity = new Domain.Entities.Service()
                {
                    /*  TODO: Change this.
                        NOTE: The Management Portal does not have the feature to select the service before initializing the call.
                        So we added the isDefault parameter to create a service with a harcoded ID. With this ID, we can
                        query a service if the request body doesn't have a specified service ID.
                    */
                    Id = request.IsDefault ? Constants.EnvironmentDefaults.ServiceId : null,
                    Name = request.FriendlyName,
                    CreatedAt = DateTime.Now,
                    State = ServiceState.Unknown,
                    Infrastructure = new Domain.Entities.Infrastructure
                    {
                        Id = resourceId,
                        IpAddress = ipAddress,
                        Dns = request.Dns,
                        PowerState = powerState,
                        ResourceGroup = request.ResourceGroup,
                        SubscriptionId = request.SubscriptionId,
                        VirtualMachineName = virtualMachineName,
                        ProvisioningDetails = provisioningDetails
                    }
                };


                await _serviceRepository.AddItemAsync(entity);

                response.Id = entity.Id;
                response.Resource = _mapper.Map<ServiceModel>(entity);

                return response;
            }

            private async Task CheckIfResourceHasBeenRegistered(string id)
            {
                var virtualMachineSpecification = new ServiceGetByInfrastructureIdSpecification(id);
                var servicesCount = await _serviceRepository.GetItemsCountAsync(virtualMachineSpecification);

                if (servicesCount > 0)
                {
                    throw new EntityNotFoundException($"The service with {id} was not found in the database");
                }
            }

            private async Task<IVirtualMachine> CheckIfResourceExistsInAzure(AddServiceCommand request)
            {
                var virtualMachine = await _virtualMachineService.GetAsync(request.SubscriptionId, request.ResourceGroup, request.Name);
                if (virtualMachine == null)
                {
                    throw new ServiceUnavailableException($"The VM {request.Name} was not found in the resource group");
                }

                return virtualMachine;
            }
        }
    }
}
