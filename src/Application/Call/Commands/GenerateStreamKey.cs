// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Call.Commands
{
    public class GenerateStreamKey
    {
        public class GenerateStreamKeyCommand : IRequest<GenerateStreamKeyCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class GenerateStreamKeyCommandResponse
        {
            public string CallId { get; set; }

            public string StreamKey { get; set; }
        }

        public class GenerateStreamKeyCommandValidator : AbstractValidator<GenerateStreamKeyCommand>
        {
            public GenerateStreamKeyCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class GenerateStreamKeyCommandHandler : IRequestHandler<GenerateStreamKeyCommand, GenerateStreamKeyCommandResponse>
        {
            private readonly ICallRepository _callRepository;
            private readonly IStreamKeyGeneratorHelper _streamKeyGeneratorHelper;

            public GenerateStreamKeyCommandHandler(
                ICallRepository callRepository,
                IStreamKeyGeneratorHelper streamKeyGeneratorHelper)
            {
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _streamKeyGeneratorHelper = streamKeyGeneratorHelper ?? throw new ArgumentNullException(nameof(streamKeyGeneratorHelper));
            }

            public async Task<GenerateStreamKeyCommandResponse> Handle(GenerateStreamKeyCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);

                if (call == null)
                {
                    throw new GenerateStreamKeyException("New stream key could not be generated", $"There is not call with id: {request.CallId} associated.");
                }

                call.PrivateContext.Remove("streamKey");
                var streamKey = _streamKeyGeneratorHelper.GetNewStreamKey();
                call.PrivateContext.Add("streamKey", streamKey);

                await _callRepository.UpdateItemAsync(call.Id, call);

                var response = new GenerateStreamKeyCommandResponse
                {
                    CallId = call.Id,
                    StreamKey = streamKey,
                };

                return response;
            }
        }
    }
}
