// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
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
    public class GetCallByMeetingId
    {
        public class GetCallByMeetingIdQuery : IRequest<GetCallByMeetingIdQueryResponse>
        {
            public string MeetingId { get; set; }

            public bool Archived { get; set; }
        }

        public class GetCallByMeetingIdQueryResponse
        {
            public CallModel Call { get; set; }
        }

        public class GetCallByMeetingIdQueryValidator : AbstractValidator<GetCallByMeetingIdQuery>
        {
            public GetCallByMeetingIdQueryValidator()
            {
                RuleFor(x => x.MeetingId)
                    .NotEmpty();
            }
        }

        public class GetCallByMeetingIdQueryHandler : IRequestHandler<GetCallByMeetingIdQuery, GetCallByMeetingIdQueryResponse>
        {
            private readonly ICallRepository _callRespository;
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly IStreamRepository _streamRepository;
            private readonly IMapper _mapper;
            private readonly ILogger<GetCallByMeetingIdQueryHandler> _logger;

            public GetCallByMeetingIdQueryHandler(
                ICallRepository callRespository,
                IParticipantStreamRepository participantStreamRepository,
                IStreamRepository streamRepository,
                IMapper mapper,
                ILogger<GetCallByMeetingIdQueryHandler> logger)
            {
                _callRespository = callRespository;
                _participantStreamRepository = participantStreamRepository;
                _streamRepository = streamRepository;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<GetCallByMeetingIdQueryResponse> Handle(GetCallByMeetingIdQuery query, CancellationToken cancellationToken)
            {
                GetCallByMeetingIdQueryResponse response = new GetCallByMeetingIdQueryResponse();

                var callSpecification = new CallGetByMeetingIdSpecification(query.MeetingId);
                var calls = await _callRespository.GetItemsAsync(callSpecification);
                var call = calls.FirstOrDefault();

                if (call == null)
                {
                    _logger.LogInformation("Call with meeting id {meetingId} was not found", query.MeetingId);
                    throw new EntityNotFoundException($"Call with meeting id {query.MeetingId} was not found");
                }

                var callModel = _mapper.Map<CallModel>(call);

                var specification = new ParticipantsStreamsGetFromCallSpecification(call.Id, query.Archived);

                // TODO: Analyze if we should change our cosmos db repository
                var participants = await _participantStreamRepository.GetItemsAsync(specification);

                var participantsModel = _mapper.Map<IEnumerable<Domain.Entities.ParticipantStream>, List<ParticipantStreamModel>>(participants);

                callModel.Streams = participantsModel;

                var streamSpecification = new StreamGetActiveFromCallSpecification(call.Id);

                var stream = await _streamRepository.GetFirstItemAsync(streamSpecification);
                if (stream != null)
                {
                    var streamModel = _mapper.Map<StreamModel>(stream);
                    callModel.InjectionStream = streamModel;
                }

                response.Call = callModel;

                return response;
            }
        }
    }
}
