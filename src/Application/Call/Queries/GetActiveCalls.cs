// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Call.Specifications;
using Application.Common.Models;
using Application.Interfaces.Persistance;
using AutoMapper;
using MediatR;

namespace Application.Call.Queries
{
    public class GetActiveCalls
    {
        public class GetActiveCallsQuery : IRequest<GetActiveCallsQueryResponse>
        {
        }

        public class GetActiveCallsQueryResponse
        {
            public List<CallModel> Calls { get; set; }
        }

        public class GetActiveCallsQueryHandler : IRequestHandler<GetActiveCallsQuery, GetActiveCallsQueryResponse>
        {
            private readonly ICallRepository _callRespository;
            private readonly IMapper _mapper;

            public GetActiveCallsQueryHandler(ICallRepository callRespository, IMapper mapper)
            {
                _callRespository = callRespository;
                _mapper = mapper;
            }

            public async Task<GetActiveCallsQueryResponse> Handle(GetActiveCallsQuery query, CancellationToken cancellationToken)
            {
                GetActiveCallsQueryResponse response = new GetActiveCallsQueryResponse();

                var specification = new CallGetActiveSpecification();

                var calls = await _callRespository.GetItemsAsync(specification);

                var callModels = _mapper.Map<IEnumerable<Domain.Entities.Call>, List<CallModel>>(calls);

                response.Calls = callModels;

                return response;
            }
        }
    }
}
