// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using FluentValidation;
using MediatR;

namespace Application.Service.Commands
{
    public class DoEndCall
    {
        public class DoEndCallCommand : IRequest<DoEndCallCommandResponse>
        {
            public string GraphCallId { get; set; }
        }

        public class DoEndCallCommandResponse
        {
            public string Id { get; set; }
        }

        public class DoEndCallCommandValidator : AbstractValidator<DoEndCallCommand>
        {
            public DoEndCallCommandValidator()
            {
                RuleFor(x => x.GraphCallId)
                    .NotEmpty();
            }
        }

        public class DoEndCallCommandHandler : IRequestHandler<DoEndCallCommand, DoEndCallCommandResponse>
        {
            private readonly IBot _bot;

            public DoEndCallCommandHandler(IBot bot)
            {
                _bot = bot;
            }

            public async Task<DoEndCallCommandResponse> Handle(DoEndCallCommand request, CancellationToken cancellationToken)
            {
                DoEndCallCommandResponse response = new DoEndCallCommandResponse();

                await _bot.EndCallAsync(request.GraphCallId);

                response.Id = request.GraphCallId;

                return response;
            }
        }
    }
}
