// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using FluentValidation;
using MediatR;

namespace Application.Service.Commands
{
    public class DoInviteBot
    {
        public class DoInviteBotCommand : IRequest<DoInviteBotCommandResponse>
        {
            public string MeetingUrl { get; set; }

            public string MeetingId { get; set; }

            public string CallId { get; set; }
        }

        public class DoInviteBotCommandResponse
        {
            public string Id { get; set; }
        }

        public class DoInviteBotCommandValidator : AbstractValidator<DoInviteBotCommand>
        {
            public DoInviteBotCommandValidator()
            {
                RuleFor(x => x.MeetingUrl)
                    .NotEmpty();
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class DoInviteBotCommandHandler : IRequestHandler<DoInviteBotCommand, DoInviteBotCommandResponse>
        {
            private readonly IBot _bot;

            public DoInviteBotCommandHandler(IBot bot)
            {
                _bot = bot;
            }

            public async Task<DoInviteBotCommandResponse> Handle(DoInviteBotCommand request, CancellationToken cancellationToken)
            {
                DoInviteBotCommandResponse response = new DoInviteBotCommandResponse();

                await _bot.InviteBotAsync(request);

                // TODO: Change response
                response.Id = request.CallId;

                return response;
            }
        }
    }
}
