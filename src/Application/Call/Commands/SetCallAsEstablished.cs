using Application.Interfaces.Persistance;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static Domain.Constants.Constants;

namespace Application.Call.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class SetCallAsEstablished
    {
        /// <summary>
        /// 
        /// </summary>
        public class SetCallAsEstablishedCommand : IRequest<SetCallAsEstablishedCommandResponse>
        {
            public string CallId { get; set; }

            public string GraphCallId { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class SetCallAsEstablishedCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SetCallAsEstablishedCommandValidator : AbstractValidator<SetCallAsEstablishedCommand>
        {
            public SetCallAsEstablishedCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
                RuleFor(x => x.GraphCallId)
                    .NotEmpty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SetCallAsEstablishedCommandHandler : IRequestHandler<SetCallAsEstablishedCommand, SetCallAsEstablishedCommandResponse>
        {
            private readonly ICallRepository callRepository;
            private readonly IParticipantStreamRepository participantStreamRepository;
            private readonly ILogger<SetCallAsEstablishedCommandHandler> logger;

            private readonly List<ParticipantStream> defaultParticipantStreams = new List<ParticipantStream>
            {
                new ParticipantStream
                {
                    ParticipantGraphId = Guid.NewGuid().ToString(),
                    DisplayName = DefaultParticipantsDisplayNames.PrimarySpeaker,
                    Type = ResourceType.PrimarySpeaker,
                    State = StreamState.Disconnected,
                    IsHealthy = true,
                    Details = new ParticipantStreamDetails()
                },
                new ParticipantStream
                {
                    ParticipantGraphId = Guid.NewGuid().ToString(),
                    DisplayName = DefaultParticipantsDisplayNames.ScreenShare,
                    Type = ResourceType.Vbss,
                    State = StreamState.Disconnected,
                    IsHealthy = true,
                    Details = new ParticipantStreamDetails()
                }
            };


            /// <summary>
            /// 
            /// </summary>
            /// <param name="callRepository"></param>
            /// <param name="logger"></param>
            public SetCallAsEstablishedCommandHandler(ICallRepository callRepository,
                IParticipantStreamRepository participantStreamRepository,
                ILogger<SetCallAsEstablishedCommandHandler> logger)
            {
                this.callRepository = callRepository;
                this.participantStreamRepository = participantStreamRepository;
                this.logger = logger;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="request"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<SetCallAsEstablishedCommandResponse> Handle(SetCallAsEstablishedCommand request, CancellationToken cancellationToken)
            {
                var response = new SetCallAsEstablishedCommandResponse();

                var entity = await callRepository.GetItemAsync(request.CallId);
                if (entity == null)
                {
                    logger.LogError("Call with id {id} was not found", request.CallId);
                    throw new EntityNotFoundException($"Call with id  {request.CallId} was not found");
                }

                entity.State = Domain.Enums.CallState.Established;
                entity.StartedAt = DateTime.UtcNow;
                entity.GraphId = request.GraphCallId;

                await callRepository.UpdateItemAsync(entity.Id, entity);
                await AddDefaultParticipantsStreams(request.CallId);

                response.Id = entity.Id;

                return response;
            }

            private async Task AddDefaultParticipantsStreams(string callId)
            {
                var insertTasks = new List<Task>();
                foreach (var participant in defaultParticipantStreams)
                {
                    participant.CallId = callId;
                    insertTasks.Add(participantStreamRepository.AddItemAsync(participant));
                }

                await Task.WhenAll(insertTasks);
            }
        }
    }
}
