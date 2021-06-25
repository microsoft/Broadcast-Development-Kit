using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Ardalis.Result;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Core.Services
{
    public class AzVirtualMachineService : IAzVirtualMachineService
    {
        private readonly IAzure _azure;
        private readonly ILogger<AzVirtualMachineService> _logger;

        private readonly List<PowerState> _transitionalStates = new List<PowerState> { PowerState.Deallocating, PowerState.Starting, PowerState.Stopping };

        public AzVirtualMachineService(
            IAzure azure,
            ILogger<AzVirtualMachineService> logger)
        {
            _azure = azure;
            _logger = logger;
        }

        public async Task<IVirtualMachine> GetAsync(string subscriptionId, string resourceGroup, string name)
        {
            var resourceId = GetVirtualMachineResourceId(subscriptionId, resourceGroup, name);
            var virtualMachine = await _azure.VirtualMachines.GetByIdAsync(resourceId);

            return virtualMachine;
        }

        public async Task<IVirtualMachine> GetByIdAsync(string id)
        {
            var virtualMachine = await _azure.VirtualMachines.GetByIdAsync(id);
            return virtualMachine;
        }

        public async Task<Result<IVirtualMachine>> StartAsync(string resourceId)
        {
            try
            {
                var virtualMachine = await GetVirtualMachineAsync(resourceId);
                if (virtualMachine == null)
                {
                    return Result<IVirtualMachine>.NotFound();
                }

                _logger.LogInformation("VM Name: {name} - State ({powerState})", virtualMachine.Name, virtualMachine.PowerState.Value);

                if (_transitionalStates.Contains(virtualMachine.PowerState))
                {
                    return Result<IVirtualMachine>.Invalid(
                        new List<ValidationError>
                        {
                            new ValidationError
                            {
                                Severity = ValidationSeverity.Info,
                                Identifier = virtualMachine.PowerState.Value,
                                ErrorMessage = $"The vm has a transitional state ({virtualMachine.PowerState.Value}), please try again in a few seconds",
                            },
                        });
                }

                if (virtualMachine.PowerState == PowerState.Unknown)
                {
                    return Result<IVirtualMachine>.Invalid(
                        new List<ValidationError>
                        {
                            new ValidationError
                            {
                                Severity = ValidationSeverity.Info,
                                Identifier = virtualMachine.PowerState.Value,
                                ErrorMessage = "The vm has an unknown state",
                            },
                        });
                }

                if (virtualMachine.PowerState == PowerState.Deallocated || virtualMachine.PowerState == PowerState.Stopped)
                {
                    _logger.LogInformation("Starting VM {name}", virtualMachine.Name);
                    try
                    {
                        await virtualMachine.StartAsync();
                        await virtualMachine.RefreshAsync();
                    }
                    catch (NullReferenceException ex) when (ex.Source == "Microsoft.Rest.ClientRuntime.Azure")
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }

                return Result<IVirtualMachine>.Success(virtualMachine);
            }
            catch (Exception ex)
            {
                return Result<IVirtualMachine>.Error(ex.Message);
            }
        }

        public async Task<Result<IVirtualMachine>> StopAsync(string resourceId)
        {
            try
            {
                var virtualMachine = await GetVirtualMachineAsync(resourceId);
                if (virtualMachine == null)
                {
                    return Result<IVirtualMachine>.NotFound();
                }

                _logger.LogInformation("VM Name: {name} - State ({powerState})", virtualMachine.Name, virtualMachine.PowerState.Value);

                if (_transitionalStates.Contains(virtualMachine.PowerState))
                {
                    return Result<IVirtualMachine>.Invalid(
                        new List<ValidationError>
                        {
                            new ValidationError
                            {
                                Identifier = virtualMachine.PowerState.Value,
                                ErrorMessage = $"The vm has a transitional state ({virtualMachine.PowerState.Value}), please try again in a few seconds",
                            },
                        });
                }

                if (virtualMachine.PowerState == PowerState.Unknown)
                {
                    return Result<IVirtualMachine>.Invalid(
                        new List<ValidationError>
                        {
                            new ValidationError
                            {
                                Identifier = virtualMachine.PowerState.Value,
                                ErrorMessage = "The vm has an unknown state",
                            },
                        });
                }

                if (virtualMachine.PowerState == PowerState.Running)
                {
                    _logger.LogInformation("Stopping VM {name}", virtualMachine.Name);
                    try
                    {
                        await virtualMachine.DeallocateAsync();
                        await virtualMachine.RefreshAsync();
                    }
                    catch (NullReferenceException ex) when (ex.Source == "Microsoft.Rest.ClientRuntime.Azure")
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }

                return Result<IVirtualMachine>.Success(virtualMachine);
            }
            catch (Exception ex)
            {
                return Result<IVirtualMachine>.Error(ex.Message);
            }
        }

        private static string GetVirtualMachineResourceId(string subscriptionId, string resourceGroup, string name)
        {
            return $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/virtualMachines/{name}";
        }

        private async Task<IVirtualMachine> GetVirtualMachineAsync(string resourceId)
        {
            _logger.LogInformation("Getting VM {resourceId}", resourceId);

            var virtualMachine = await _azure.VirtualMachines.GetByIdAsync(resourceId);

            return virtualMachine;
        }
    }
}
