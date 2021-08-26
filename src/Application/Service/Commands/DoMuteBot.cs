// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using MediatR;

namespace Application.Service.Commands
{
    public class DoMuteBot
    {
        public class DoMuteBotCommand : IRequest<DoMuteBotCommandResponse>
        {
        }

        public class DoMuteBotCommandResponse
        {
        }

        public class DoMuteBotCommandHandler : IRequestHandler<DoMuteBotCommand, DoMuteBotCommandResponse>
        {
            private readonly IBot _bot;

            public DoMuteBotCommandHandler(IBot bot)
            {
                _bot = bot;
            }

            public async Task<DoMuteBotCommandResponse> Handle(DoMuteBotCommand request, CancellationToken cancellationToken)
            {
                await _bot.MuteBotAsync();
                return null;
            }
        }
    }
}
