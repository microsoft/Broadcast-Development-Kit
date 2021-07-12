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
    public class MuteBotFromCall
    {
        public class MuteBotFromCallCommand : IRequest<MuteBotFromCallCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class MuteBotFromCallCommandResponse
        {
        }

        public class MuteBotFromCallCommandHandler : IRequestHandler<MuteBotFromCallCommand, MuteBotFromCallCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;

            public MuteBotFromCallCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            }

            public async Task<MuteBotFromCallCommandResponse> Handle(MuteBotFromCallCommand request, CancellationToken cancellationToken)
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
