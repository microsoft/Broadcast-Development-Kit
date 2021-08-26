// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using MediatR;

namespace Application.Stream.Commands
{
    public class RequestMuteBot
    {
        public class RequestMuteBotCommand : IRequest<RequestMuteBotCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class RequestMuteBotCommandResponse
        {
        }

        public class RequestMuteBotCommandHandler : IRequestHandler<RequestMuteBotCommand, RequestMuteBotCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;

            public RequestMuteBotCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            }

            public async Task<RequestMuteBotCommandResponse> Handle(RequestMuteBotCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);
                var service = await _serviceRepository.GetItemAsync(call.ServiceId);
                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);
                await _botServiceClient.MuteBotAsync();
                return null;
            }
        }
    }
}
