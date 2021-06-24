using Application.Interfaces.Persistance;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Call.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class SetCallAsTerminated
    {
        /// <summary>
        /// 
        /// </summary>
        public class SetCallAsTerminatedCommand : IRequest<SetCallAsTerminatedCommandResponse>
        {
            public string CallId { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class SetCallAsTerminatedCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SetCallAsTerminatedCommandValidator : AbstractValidator<SetCallAsTerminatedCommand>
        {
            public SetCallAsTerminatedCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SetCallAsTerminatedCommandHandler : IRequestHandler<SetCallAsTerminatedCommand, SetCallAsTerminatedCommandResponse>
        {
            private readonly ICallRepository callRepository;
            private readonly ILogger<SetCallAsTerminatedCommandHandler> logger;

            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="callRepository"></param>
            /// <param name="logger"></param>
            public SetCallAsTerminatedCommandHandler(ICallRepository callRepository,
                ILogger<SetCallAsTerminatedCommandHandler> logger)
            {
                this.callRepository = callRepository;
                this.logger = logger;
            }

            public async Task<SetCallAsTerminatedCommandResponse> Handle(SetCallAsTerminatedCommand request, CancellationToken cancellationToken)
            {
                var response = new SetCallAsTerminatedCommandResponse();

                var entity = await callRepository.GetItemAsync(request.CallId);
                if (entity == null)
                {
                    logger.LogError("Call with id {id} was not found", request.CallId);
                    throw new EntityNotFoundException($"Call with id  {request.CallId} was not found");
                }

                entity.State = Domain.Enums.CallState.Terminated;
                entity.EndedAt = DateTime.UtcNow;
                await callRepository.UpdateItemAsync(entity.Id, entity);

                response.Id = entity.Id;

                return response;
            }
        }
    }
}
