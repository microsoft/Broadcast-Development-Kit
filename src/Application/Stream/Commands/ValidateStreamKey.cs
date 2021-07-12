// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Interfaces.Persistance;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Stream.Commands
{
    public class ValidateStreamKey
    {
        public class ValidateStreamKeyCommand : IRequest<ValidateStreamKeyCommandResponse>
        {
            public string CallId { get; set; }

            public string StreamKey { get; set; }
        }

        public class ValidateStreamKeyCommandResponse
        {
        }

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

        public class ValidateStreamKeyCommandHandler : IRequestHandler<ValidateStreamKeyCommand, ValidateStreamKeyCommandResponse>
        {
            private readonly ICallRepository _callRepository;

            public ValidateStreamKeyCommandHandler(ICallRepository callRepository)
            {
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
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
