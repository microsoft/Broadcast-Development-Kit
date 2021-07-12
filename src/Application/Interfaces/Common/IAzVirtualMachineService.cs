// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading.Tasks;
using Ardalis.Result;
using Microsoft.Azure.Management.Compute.Fluent;

namespace Application.Interfaces.Common
{
    public interface IAzVirtualMachineService
    {
        Task<IVirtualMachine> GetAsync(string subscriptionId, string resourceGroup, string name);

        Task<IVirtualMachine> GetByIdAsync(string id);

        Task<Result<IVirtualMachine>> StartAsync(string resourceId);

        Task<Result<IVirtualMachine>> StopAsync(string resourceId);
    }
}
