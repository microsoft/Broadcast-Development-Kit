using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Call.Specifications;
using Application.Common.Models;
using Application.Interfaces.Persistance;
using Application.Participants.Specifications;
using Application.Stream.Specifications;
using AutoMapper;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Call.Queries
{
    /// <summary>
    ///     Get related query, validators, and handlers
    /// </summary>
    public class GetCallByMeetingId
    {
        /// <summary>
        ///     Model to Get an entity
        /// </summary>
        public class GetCallByMeetingIdQuery : IRequest<GetCallByMeetingIdQueryResponse>
        {
            /// <summary>
            ///     Id
            /// </summary>
            public string MeetingId { get; set; }
            public bool Archived { get; set; }

        }

        /// <summary>
        ///     Query Response
        /// </summary>
        public class GetCallByMeetingIdQueryResponse
        {
            /// <summary>
            ///     Call
            /// </summary>
            public CallModel Call { get; set; }
        }

        /// <summary>
        ///     Register Validation 
        /// </summary>
        public class GetCallByMeetingIdQueryValidator : AbstractValidator<GetCallByMeetingIdQuery>
        {
            /// <summary>
            ///     Validator ctor
            /// </summary>
            public GetCallByMeetingIdQueryValidator()
            {
                RuleFor(x => x.MeetingId)
                    .NotEmpty();
            }

        }


        /// <summary>
        ///     Handler
        /// </summary>
        public class GetCallByMeetingIdQueryHandler : IRequestHandler<GetCallByMeetingIdQuery, GetCallByMeetingIdQueryResponse>
        {
            private readonly ICallRepository callRespository;
            private readonly IParticipantStreamRepository participantStreamRepository;
            private readonly IStreamRepository streamRepository;
            private readonly IMapper mapper;
            private readonly ILogger<GetCallByMeetingIdQueryHandler> logger;

            /// <summary>
            ///     Ctor
            /// </summary>
            /// <param name="callRespository"></param>
            /// <param name="mapper"></param>
            /// <param name="logger"></param>
            public GetCallByMeetingIdQueryHandler(ICallRepository callRespository,
                                  IParticipantStreamRepository participantStreamRepository,
                                  IStreamRepository streamRepository,
                                  IMapper mapper,
                                  ILogger<GetCallByMeetingIdQueryHandler> logger)
            {
                this.callRespository = callRespository;
                this.participantStreamRepository = participantStreamRepository;
                this.streamRepository = streamRepository;
                this.mapper = mapper;
                this.logger = logger;
            }

            /// <summary>
            ///     Handle
            /// </summary>
            /// <param name="query"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<GetCallByMeetingIdQueryResponse> Handle(GetCallByMeetingIdQuery query, CancellationToken cancellationToken)
            {
                GetCallByMeetingIdQueryResponse response = new GetCallByMeetingIdQueryResponse();

                var callSpecification = new CallGetByMeetingIdSpecification(query.MeetingId);
                var calls = await callRespository.GetItemsAsync(callSpecification);
                var call = calls.FirstOrDefault();

                if (call == null)
                {
                    logger.LogInformation("Call with meeting id {meetingId} was not found", query.MeetingId);
                    throw new EntityNotFoundException($"Call with meeting id {query.MeetingId} was not found");
                }

                var callModel = mapper.Map<CallModel>(call);

                var specification = new ParticipantsStreamsGetFromCallSpecification(call.Id, query.Archived);

                //TODO: Analyze if we should change our cosmos db repository
                var participants = await this.participantStreamRepository.GetItemsAsync(specification);

                var participantsModel = mapper.Map<IEnumerable<Domain.Entities.ParticipantStream>,List<ParticipantStreamModel>>(participants);

                callModel.Streams = participantsModel;

                var streamSpecification = new StreamGetActiveFromCallSpecification(call.Id);

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
