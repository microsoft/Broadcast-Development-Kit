// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Domain.Exceptions;
using MediatR;

namespace Application.Service.Commands
{
    public class DoUnmuteBot
    {
        public class DoUnmuteBotCommand : IRequest<DoUnmuteBotCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class DoUnmuteBotCommandResponse
        {
        }

        public class DoUnmuteBotCommandHandler : IRequestHandler<DoUnmuteBotCommand, DoUnmuteBotCommandResponse>
        {
            private readonly IBot _bot;
            private readonly ICallRepository _callRepository;

            public DoUnmuteBotCommandHandler(
                IBot bot,
                ICallRepository callRepository)
            {
                _bot = bot;
                _callRepository = callRepository;
            }

            public async Task<DoUnmuteBotCommandResponse> Handle(DoUnmuteBotCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);

                if (call == null)
                {
                    throw new EntityNotFoundException(nameof(Call), request.CallId);
                }

                await _bot.UnmuteBotAsync();

                call.IsBotMuted = false;

                await _callRepository.UpdateItemAsync(call.Id, call);

                return null;
            }
        }
    }
}
