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
        /// <summary>
        ///     Model to Get an entity
        /// </summary>
        public class GetActiveCallsQuery : IRequest<GetActiveCallsQueryResponse>
        {

        }

        /// <summary>
        ///     Query Response
        /// </summary>
        public class GetActiveCallsQueryResponse
        {
            /// <summary>
            ///     Call
            /// </summary>
            public List<CallModel> Calls { get; set; }
        }

        /// <summary>
        ///     Handler
        /// </summary>
        public class GetActiveCallsQueryHandler : IRequestHandler<GetActiveCallsQuery, GetActiveCallsQueryResponse>
        {
            private readonly ICallRepository callRespository;
            private readonly IMapper mapper;

            /// <summary>
            ///     Ctor
            /// </summary>
            /// <param name="callRespository"></param>
            /// <param name="mapper"></param>
            /// <param name="logger"></param>
            public GetActiveCallsQueryHandler(ICallRepository callRespository, IMapper mapper)
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
            public async Task<GetActiveCallsQueryResponse> Handle(GetActiveCallsQuery query, CancellationToken cancellationToken)
            {
                GetActiveCallsQueryResponse response = new GetActiveCallsQueryResponse();

                var specification = new CallGetActiveSpecification();

                var calls = await callRespository.GetItemsAsync(specification);

                var callModels = mapper.Map<IEnumerable<Domain.Entities.Call>, List<CallModel>>(calls);

                response.Calls = callModels;

                return response;
            }
        }
    }
}
