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
    public class RequestMuteBotFromCall
    {
        public class RequestMuteBotFromCallCommand : IRequest<RequestMuteBotFromCallCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class RequestMuteBotFromCallCommandResponse
        {
        }

        public class RequestMuteBotFromCallCommandHandler : IRequestHandler<RequestMuteBotFromCallCommand, RequestMuteBotFromCallCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;

            public RequestMuteBotFromCallCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            }

            public async Task<RequestMuteBotFromCallCommandResponse> Handle(RequestMuteBotFromCallCommand request, CancellationToken cancellationToken)
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
