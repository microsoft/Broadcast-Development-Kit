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
        /// <summary>
        ///     Model to Get an entity
        /// </summary>
        public class GetArchivedCallsQuery : IRequest<GetArchivedCallsQueryResponse>
        {
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        /// <summary>
        ///     Query Response
        /// </summary>
        public class GetArchivedCallsQueryResponse
        {
            public PagedQueryResult<CallModel> Result { get; set; } = new PagedQueryResult<CallModel>();
        }

        /// <summary>
        ///     Handler
        /// </summary>
        public class GetArchivedCallsQueryHandler : IRequestHandler<GetArchivedCallsQuery, GetArchivedCallsQueryResponse>
        {
            private readonly ICallRepository callRespository;
            private readonly IMapper mapper;

            /// <summary>
            ///     Ctor
            /// </summary>
            /// <param name="callRespository"></param>
            /// <param name="mapper"></param>
            /// <param name="logger"></param>
            public GetArchivedCallsQueryHandler(ICallRepository callRespository, IMapper mapper)
            {
                this.callRespository = callRespository;
                this.mapper = mapper;
            }

            /// <summary>
            ///     Handle
            /// </summary>
            /// <param name="query"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<GetArchivedCallsQueryResponse> Handle(GetArchivedCallsQuery query, CancellationToken cancellationToken)
            {
                GetArchivedCallsQueryResponse response = new GetArchivedCallsQueryResponse();

                //Records
                var specification = new CallGetArchivedSpecification(query.PageNumber, query.PageSize);

                var calls = await callRespository.GetItemsAsync(specification);

                var callModels = mapper.Map<IEnumerable<Domain.Entities.Call>, List<CallModel>>(calls);

                response.Result.Items = callModels;

                // count
                CallGetArchivedAggregationSpecification countSpecification = new CallGetArchivedAggregationSpecification();
                var count = await callRespository.GetItemsCountAsync(countSpecification);
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