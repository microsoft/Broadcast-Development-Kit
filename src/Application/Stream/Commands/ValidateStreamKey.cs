using Application.Exceptions;
using Application.Interfaces.Persistance;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Stream.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidateStreamKey
    {
        /// <summary>
        /// 
        /// </summary>
        public class ValidateStreamKeyCommand : IRequest<ValidateStreamKeyCommandResponse>
        {
            public string CallId { get; set; }
            public string StreamKey { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ValidateStreamKeyCommandResponse
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public class ValidateStreamKeyCommandValidator : AbstractValidator<ValidateStreamKeyCommand>
        {
            public ValidateStreamKeyCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
                RuleFor(x => x.StreamKey)
                    .NotEmpty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ValidateStreamKeyCommandHandler : IRequestHandler<ValidateStreamKeyCommand, ValidateStreamKeyCommandResponse>
        {
            private readonly ICallRepository _callRepository;
            private readonly ILogger<ValidateStreamKeyCommandHandler> _logger;

            public ValidateStreamKeyCommandHandler(
                ICallRepository callRepository,
                ILogger<ValidateStreamKeyCommandHandler> logger
                )
            {
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<ValidateStreamKeyCommandResponse> Handle(ValidateStreamKeyCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);

                if (call == null)
                {
                    throw new NotValidStreamKeyException("Stream Key validation failed", $"There is not call with id: {request.CallId} associated.");
                }

                if (!call.PrivateContext.TryGetValue("streamKey", out string streamKey))
                {
                    throw new NotValidStreamKeyException("Stream Key validation failed", $"Stream key not configured for call with id: {call.Id}.");
                }

                if (streamKey != request.StreamKey)
                {
                    throw new NotValidStreamKeyException("Stream Key validation failed", "The Stream Key provided is not valid.");
                }

                return new ValidateStreamKeyCommandResponse();
            }

        }
    }
}
