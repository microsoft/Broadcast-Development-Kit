// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using FluentValidation;
using MediatR;

namespace Application.Service.Commands
{
    public class RemoveBot
    {
        public class RemoveBotCommand : IRequest<RemoveBotCommandResponse>
        {
            public string GraphCallId { get; set; }
        }

        public class RemoveBotCommandResponse
        {
            public string Id { get; set; }
        }

        public class RemoveBotCommandValidator : AbstractValidator<RemoveBotCommand>
        {
            public RemoveBotCommandValidator()
            {
                RuleFor(x => x.GraphCallId)
                    .NotEmpty();
            }
        }

        public class RemoveBotCommandHandler : IRequestHandler<RemoveBotCommand, RemoveBotCommandResponse>
        {
            private readonly IBot _bot;

            public RemoveBotCommandHandler(IBot bot)
            {
                _bot = bot;
            }

            public async Task<RemoveBotCommandResponse> Handle(RemoveBotCommand request, CancellationToken cancellationToken)
            {
                RemoveBotCommandResponse response = new RemoveBotCommandResponse();

                await _bot.RemoveBotAsync(request.GraphCallId);

                response.Id = request.GraphCallId;

                return response;
            }
        }
    }
}
