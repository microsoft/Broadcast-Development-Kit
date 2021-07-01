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
    public class UnmuteBotFromCall
    {
        public class UnmuteBotFromCallCommand : IRequest<UnmuteBotFromCallCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class UnmuteBotFromCallCommandResponse
        {
        }

        public class UnmuteBotFromCallCommandHandler : IRequestHandler<UnmuteBotFromCallCommand, UnmuteBotFromCallCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;

            public UnmuteBotFromCallCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            }

            public async Task<UnmuteBotFromCallCommandResponse> Handle(UnmuteBotFromCallCommand request, CancellationToken cancellationToken)
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
