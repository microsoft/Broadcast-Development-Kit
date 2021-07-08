// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Constants;
using Domain.Exceptions;
using MediatR;

namespace Application.Service.Queries
{
    public class GetService
    {
        public class GetServiceQuery : IRequest<GetServiceQueryResponse>
        {
            public string ServiceId { get; set; }
        }

        public class GetServiceQueryResponse
        {
            public string Id { get; set; }

            public ServiceModel Resource { get; set; }
        }

        public class GetServiceQueryHandler : IRequestHandler<GetServiceQuery, GetServiceQueryResponse>
        {
            private readonly IServiceRepository _serviceRepository;
            private readonly IMapper _mapper;

            public GetServiceQueryHandler(
                IServiceRepository serviceRepository,
                IMapper mapper)
            {
                _serviceRepository = serviceRepository ?? throw new System.ArgumentNullException(nameof(serviceRepository));
                _mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            }

            public async Task<GetServiceQueryResponse> Handle(GetServiceQuery query, CancellationToken cancellationToken)
            {
                /* TODO: Change this.
                   NOTE: The Management Portal does not have the feature to select the service before initializing the call.
                   The folloiwng code is temporary, if the service Id is not specified, we use a harcoded ID to retrieve the service.
                */

                var serviceId = string.IsNullOrEmpty(query.ServiceId) ? Constants.EnvironmentDefaults.ServiceId : query.ServiceId;
                var entity = await _serviceRepository.GetItemAsync(serviceId);

                if (entity == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Service), serviceId);
                }

                var response = new GetServiceQueryResponse
                {
                    Id = entity.Id,
                    Resource = _mapper.Map<ServiceModel>(entity),
                };

                return response;
            }
        }
    }
}
