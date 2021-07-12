// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
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
    public class GetCall
    {
        public class GetCallQuery : IRequest<GetCallQueryResponse>
        {
            public string Id { get; set; }

            public bool Archived { get; set; }
        }

        public class GetCallQueryResponse
        {
            public CallModel Call { get; set; }
        }

        public class GetCallQueryValidator : AbstractValidator<GetCallQuery>
        {
            public GetCallQueryValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty();
            }
        }

        public class GetCallQueryHandler : IRequestHandler<GetCallQuery, GetCallQueryResponse>
        {
            private readonly ICallRepository _callRepository;
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly IStreamRepository _streamRepository;
            private readonly IMapper _mapper;

            public GetCallQueryHandler(
                ICallRepository callRepository,
                IParticipantStreamRepository participantStreamRepository,
                IStreamRepository streamRepository,
                IMapper mapper)
            {
                _callRepository = callRepository;
                _participantStreamRepository = participantStreamRepository;
                _streamRepository = streamRepository;
                _mapper = mapper;
            }

            public async Task<GetCallQueryResponse> Handle(GetCallQuery query, CancellationToken cancellationToken)
            {
                GetCallQueryResponse response = new GetCallQueryResponse();

                Domain.Entities.Call entity = await _callRepository.GetItemAsync(query.Id);
                if (entity == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Call), query.Id);
                }

                var callModel = _mapper.Map<CallModel>(entity);

                var specification = new ParticipantsStreamsGetFromCallSpecification(query.Id, query.Archived);

                // TODO: Analyze if we should change our cosmos db repository
                var participants = await _participantStreamRepository.GetItemsAsync(specification);

                var participantsModel = _mapper.Map<IEnumerable<Domain.Entities.ParticipantStream>, List<ParticipantStreamModel>>(participants);

                callModel.Streams = participantsModel;

                var streamSpecification = new StreamGetActiveFromCallSpecification(query.Id);

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
