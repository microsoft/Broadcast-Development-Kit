using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Persistance;
using Application.Participants.Specifications;
using Application.Stream.Specifications;
using AutoMapper;
using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Call.Queries
{
    /// <summary>
    ///     Get related query, validators, and handlers
    /// </summary>
    public class GetCall
    {
        /// <summary>
        ///     Model to Get an entity
        /// </summary>
        public class GetCallQuery : IRequest<GetCallQueryResponse>
        {
            /// <summary>
            ///     Id
            /// </summary>
            public string Id { get; set; }
            public bool Archived { get; set; }

        }

        /// <summary>
        ///     Query Response
        /// </summary>
        public class GetCallQueryResponse
        {
            /// <summary>
            ///     Call
            /// </summary>
            public CallModel Call { get; set; }
        }

        /// <summary>
        ///     Register Validation 
        /// </summary>
        public class GetCallQueryValidator : AbstractValidator<GetCallQuery>
        {
            /// <summary>
            ///     Validator ctor
            /// </summary>
            public GetCallQueryValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty();
            }

        }


        /// <summary>
        ///     Handler
        /// </summary>
        public class GetCallQueryHandler : IRequestHandler<GetCallQuery, GetCallQueryResponse>
        {
            private readonly ICallRepository callRespository;
            private readonly IParticipantStreamRepository participantStreamRepository;
            private readonly IStreamRepository streamRepository;
            private readonly IMapper mapper;

            /// <summary>
            ///     Ctor
            /// </summary>
            /// <param name="callRespository"></param>
            /// <param name="mapper"></param>
            /// <param name="logger"></param>
            public GetCallQueryHandler(ICallRepository callRespository,
                                  IParticipantStreamRepository participantStreamRepository,
                                  IStreamRepository streamRepository,
                                  IMapper mapper)
            {
                this.callRespository = callRespository;
                this.participantStreamRepository = participantStreamRepository;
                this.streamRepository = streamRepository;
                this.mapper = mapper;
            }

            /// <summary>
            ///     Handle
            /// </summary>
            /// <param name="query"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<GetCallQueryResponse> Handle(GetCallQuery query, CancellationToken cancellationToken)
            {
                GetCallQueryResponse response = new GetCallQueryResponse();

                Domain.Entities.Call entity = await callRespository.GetItemAsync(query.Id);
                if (entity == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Call), query.Id);
                }

                var callModel = mapper.Map<CallModel>(entity);

                var specification = new ParticipantsStreamsGetFromCallSpecification(query.Id, query.Archived);

                //TODO: Analyze if we should change our cosmos db repository
                var participants = await this.participantStreamRepository.GetItemsAsync(specification);

                var participantsModel = mapper.Map<IEnumerable<Domain.Entities.ParticipantStream>, List<ParticipantStreamModel>>(participants);

                callModel.Streams = participantsModel;

                var streamSpecification = new StreamGetActiveFromCallSpecification(query.Id);

                var stream = await this.streamRepository.GetFirstItemAsync(streamSpecification);
                if (stream != null)
                {
                    var streamModel = mapper.Map<StreamModel>(stream);
                    callModel.InjectionStream = streamModel;
                }

                response.Call = callModel;

                return response;
            }
        }
    }
}
