using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Call.Specifications;
using Application.Common.Models;
using Application.Interfaces.Persistance;
using Application.Participants.Specifications;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Call.Queries
{
    public class GetPublicCallForParticipantByMeetingId
    {
        public class GetPublicCallForParticipantByMeetingIdQuery : IRequest<GetPublicCallForParticipantByMeetingIdResponse>
        {
            public string MeetingId { get; set; }

            public ResourceType ResourceType { get; set; }

            public string ParticipantAadId { get; set; }
        }

        public class GetPublicCallForParticipantByMeetingIdResponse
        {
            public PublicCallModelForParticipant CallModelForParticipant { get; set; }
        }

        public class GetPublicCallForParticipantByMeetingIdQueryValidator : AbstractValidator<GetPublicCallForParticipantByMeetingIdQuery>
        {
            public GetPublicCallForParticipantByMeetingIdQueryValidator()
            {
                RuleFor(x => x.MeetingId)
                    .NotEmpty();
                RuleFor(x => x.ResourceType)
                    .NotNull()
                    .Must(p => p == ResourceType.PrimarySpeaker || p == ResourceType.Participant || p == ResourceType.Vbss)
                    .WithMessage("Resource type not supported");
                When(p => p.ResourceType == ResourceType.Participant, () =>
                {
                    RuleFor(x => x.ParticipantAadId)
                        .NotEmpty();
                });
            }
        }

        public class GetPublicCallForParticipantByMeetingIdHandler : IRequestHandler<GetPublicCallForParticipantByMeetingIdQuery, GetPublicCallForParticipantByMeetingIdResponse>
        {
            private readonly ICallRepository _callRespository;
            private readonly IParticipantStreamRepository _participantStreamRepository;
            private readonly ILogger<GetPublicCallForParticipantByMeetingIdHandler> _logger;

            public GetPublicCallForParticipantByMeetingIdHandler(
                ICallRepository callRespository,
                IParticipantStreamRepository participantStreamRepository,
                ILogger<GetPublicCallForParticipantByMeetingIdHandler> logger)
            {
                _callRespository = callRespository ?? throw new ArgumentNullException(nameof(callRespository));
                _participantStreamRepository = participantStreamRepository ?? throw new ArgumentNullException(nameof(participantStreamRepository));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<GetPublicCallForParticipantByMeetingIdResponse> Handle(GetPublicCallForParticipantByMeetingIdQuery query, CancellationToken cancellationToken)
            {
                var callSpecification = new CallGetByMeetingIdSpecification(query.MeetingId);
                var calls = await _callRespository.GetItemsAsync(callSpecification);
                var call = calls.FirstOrDefault();

                if (call == null)
                {
                    _logger.LogInformation("Call with meeting id {meetingId} was not found", query.MeetingId);
                    throw new EntityNotFoundException($"Call with meeting id {query.MeetingId} was not found");
                }

                var participantSpecification = new ParticipantStreamsGetFromCallSpecification(call.Id, query.ResourceType, query.ParticipantAadId);

                // TODO: Analyze if we should change our cosmos db repository
                var participants = await _participantStreamRepository.GetItemsAsync(participantSpecification);
                var participant = participants.FirstOrDefault();

                if (participant == null)
                {
                    _logger.LogInformation("Participant with type {type} and AAD id {participantAadId} was not found", query.ResourceType, query.ParticipantAadId);
                    throw new EntityNotFoundException($"Participant with type {query.ResourceType} and AAD id {query.ParticipantAadId} was not found");
                }

                // Do not return any personal information of the participant in this response.
                return new GetPublicCallForParticipantByMeetingIdResponse
                {
                    CallModelForParticipant = new PublicCallModelForParticipant
                    {
                        State = call.State,
                        StreamState = participant.State,
                        PublicContext = call.PublicContext,
                    },
                };
            }
        }
    }
}
