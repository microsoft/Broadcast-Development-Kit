// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using MediatR;

namespace Application.Service.Commands
{
    public class DoUnmuteBot
    {
        public class DoUnmuteBotCommand : IRequest<DoUnmuteBotCommandResponse>
        {
        }

        public class DoUnmuteBotCommandResponse
        {
            public string Id { get; set; }
        }

        public class DoUnmuteBotCommandHandler : IRequestHandler<DoUnmuteBotCommand, DoUnmuteBotCommandResponse>
        {
            private readonly IBot _bot;

            public DoUnmuteBotCommandHandler(IBot bot)
            {
                _bot = bot;
            }

            public async Task<DoUnmuteBotCommandResponse> Handle(DoUnmuteBotCommand request, CancellationToken cancellationToken)
            {
                var response = new DoUnmuteBotCommandResponse();

                await _bot.UnmuteBotAsync();

                return response;
            }
        }
    }
}
