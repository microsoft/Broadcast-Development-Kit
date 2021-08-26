// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using MediatR;

namespace Application.Call.Commands
{
    public class RequestUnmuteBot
    {
        public class RequestUnmuteBotCommand : IRequest<RequestUnmuteBotCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class RequestUnmuteBotCommandResponse
        {
        }

        public class RequestUnmuteBotCommandHandler : IRequestHandler<RequestUnmuteBotCommand, RequestUnmuteBotCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;

            public RequestUnmuteBotCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            }

            public async Task<RequestUnmuteBotCommandResponse> Handle(RequestUnmuteBotCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);
                var service = await _serviceRepository.GetItemAsync(call.ServiceId);
                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);
                await _botServiceClient.UnmuteBotAsync();
                return null;
            }
        }
    }
}
