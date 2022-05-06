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
    public class DoMuteBot
    {
        public class DoMuteBotCommand : IRequest<DoMuteBotCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class DoMuteBotCommandResponse
        {
        }

        public class DoMuteBotCommandHandler : IRequestHandler<DoMuteBotCommand, DoMuteBotCommandResponse>
        {
            private readonly IBot _bot;
            private readonly ICallRepository _callRepository;

            public DoMuteBotCommandHandler(
                IBot bot,
                ICallRepository callRepository)
            {
                _bot = bot;
                _callRepository = callRepository;
            }

            public async Task<DoMuteBotCommandResponse> Handle(DoMuteBotCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);

                if (call == null)
                {
                    throw new EntityNotFoundException(nameof(Call), request.CallId);
                }

                await _bot.MuteBotAsync();

                call.IsBotMuted = true;

                await _callRepository.UpdateItemAsync(call.Id, call);

                return null;
            }
        }
    }
}
