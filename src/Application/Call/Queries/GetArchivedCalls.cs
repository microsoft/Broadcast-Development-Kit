using System;
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
    public class GetArchivedCalls
    {
        public class GetArchivedCallsQuery : IRequest<GetArchivedCallsQueryResponse>
        {
            public int PageNumber { get; set; }

            public int PageSize { get; set; }
        }

        public class GetArchivedCallsQueryResponse
        {
            public PagedQueryResult<CallModel> Result { get; set; } = new PagedQueryResult<CallModel>();
        }

        public class GetArchivedCallsQueryHandler : IRequestHandler<GetArchivedCallsQuery, GetArchivedCallsQueryResponse>
        {
            private readonly ICallRepository _callRespository;
            private readonly IMapper _mapper;

            public GetArchivedCallsQueryHandler(ICallRepository callRespository, IMapper mapper)
            {
                _callRespository = callRespository;
                _mapper = mapper;
            }

            public async Task<GetArchivedCallsQueryResponse> Handle(GetArchivedCallsQuery query, CancellationToken cancellationToken)
            {
                GetArchivedCallsQueryResponse response = new GetArchivedCallsQueryResponse();

                // Records
                var specification = new CallGetArchivedSpecification(query.PageNumber, query.PageSize);

                var calls = await _callRespository.GetItemsAsync(specification);

                var callModels = _mapper.Map<IEnumerable<Domain.Entities.Call>, List<CallModel>>(calls);

                response.Result.Items = callModels;

                // count
                CallGetArchivedAggregationSpecification countSpecification = new CallGetArchivedAggregationSpecification();
                var count = await _callRespository.GetItemsCountAsync(countSpecification);
                var totalPages = (int)Math.Ceiling((0D + count) / query.PageSize);

                response.Result.PageSize = query.PageSize;
                response.Result.CurrentPage = query.PageNumber;
                response.Result.TotalItems = count;
                response.Result.TotalPages = totalPages;

                return response;
            }
        }
    }
}