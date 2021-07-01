// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using MediatR;

namespace Application.Service.Commands
{
    public class MuteBot
    {
        public class MuteBotCommand : IRequest<MuteBotCommandResponse>
        {
        }

        public class MuteBotCommandResponse
        {
        }

        public class MuteBotCommandHandler : IRequestHandler<MuteBotCommand, MuteBotCommandResponse>
        {
            private readonly IBot _bot;

            public MuteBotCommandHandler(IBot bot)
            {
                _bot = bot;
            }

            public async Task<MuteBotCommandResponse> Handle(MuteBotCommand request, CancellationToken cancellationToken)
            {
                await _bot.MuteBotAsync();
                return null;
            }
        }
    }
}
