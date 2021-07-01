// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using FluentValidation;
using MediatR;

namespace Application.Service.Commands
{
    public class InviteBot
    {
        public class InviteBotCommand : IRequest<InviteBotCommandResponse>
        {
            public string MeetingUrl { get; set; }

            public string MeetingId { get; set; }

            public string CallId { get; set; }
        }

        public class InviteBotCommandResponse
        {
            public string Id { get; set; }
        }

        public class InviteBotCommandValidator : AbstractValidator<InviteBotCommand>
        {
            public InviteBotCommandValidator()
            {
                RuleFor(x => x.MeetingUrl)
                    .NotEmpty();
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class InviteBotCommandHandler : IRequestHandler<InviteBotCommand, InviteBotCommandResponse>
        {
            private readonly IBot _bot;

            public InviteBotCommandHandler(IBot bot)
            {
                _bot = bot;
            }

            public async Task<InviteBotCommandResponse> Handle(InviteBotCommand request, CancellationToken cancellationToken)
            {
                InviteBotCommandResponse response = new InviteBotCommandResponse();

                await _bot.InviteBotAsync(request);

                // TODO: Change response
                response.Id = request.CallId;

                return response;
            }
        }
    }
}
