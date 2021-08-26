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
    public class RequestUnmuteBotFromCall
    {
        public class RequestUnmuteBotFromCallCommand : IRequest<RequestUnmuteBotFromCallCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class RequestUnmuteBotFromCallCommandResponse
        {
        }

        public class RequestUnmuteBotFromCallCommandHandler : IRequestHandler<RequestUnmuteBotFromCallCommand, RequestUnmuteBotFromCallCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;

            public RequestUnmuteBotFromCallCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            }

            public async Task<RequestUnmuteBotFromCallCommandResponse> Handle(RequestUnmuteBotFromCallCommand request, CancellationToken cancellationToken)
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
