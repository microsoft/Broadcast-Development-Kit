// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Ardalis.Specification;

namespace Application.Service.Specifications
{
    public class ServiceGetByInfrastructureIdSpecification : Specification<Domain.Entities.Service>
    {
        public ServiceGetByInfrastructureIdSpecification(string id)
        {
            Query.Where(x => x.Infrastructure.Id == id);
        }
    }
}
