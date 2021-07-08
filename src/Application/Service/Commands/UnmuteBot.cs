// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using MediatR;

namespace Application.Service.Commands
{
    public class UnmuteBot
    {
        public class UnmuteBotCommand : IRequest<UnmuteBotCommandResponse>
        {
        }

        public class UnmuteBotCommandResponse
        {
            public string Id { get; set; }
        }

        public class UnmuteBotCommandHandler : IRequestHandler<UnmuteBotCommand, UnmuteBotCommandResponse>
        {
            private readonly IBot _bot;

            public UnmuteBotCommandHandler(IBot bot)
            {
                _bot = bot;
            }

            public async Task<UnmuteBotCommandResponse> Handle(UnmuteBotCommand request, CancellationToken cancellationToken)
            {
                var response = new UnmuteBotCommandResponse();

                await _bot.UnmuteBotAsync();

                return response;
            }
        }
    }
}
