// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Ardalis.Specification;

namespace Application.Service.Specifications
{
    public class ServiceGetByVirtualMachineNameSpecification : Specification<Domain.Entities.Service>
    {
        public ServiceGetByVirtualMachineNameSpecification(string virtualMachineName)
        {
            Query.Where(x =>
                x.Infrastructure.VirtualMachineName == virtualMachineName);
        }
    }
}
