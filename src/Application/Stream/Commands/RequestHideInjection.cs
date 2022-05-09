// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Application.Stream.Specifications;
using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Stream.Commands
{
    public class RequestHideInjection
    {
        public class RequestHideInjectionCommand : IRequest<RequestHideInjectionCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class RequestHideInjectionCommandResponse
        {
        }

        public class RequestHideInjectionCommandValidator : AbstractValidator<RequestHideInjectionCommand>
        {
            public RequestHideInjectionCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class RequestHideInjectionCommandHandler : IRequestHandler<RequestHideInjectionCommand, RequestHideInjectionCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IStreamRepository _streamRepository;

            public RequestHideInjectionCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository,
                IStreamRepository streamRepository)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
                _streamRepository = streamRepository ?? throw new ArgumentNullException(nameof(streamRepository));
            }

            public async Task<RequestHideInjectionCommandResponse> Handle(RequestHideInjectionCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);
                if (call == null)
                {
                    throw new EntityNotFoundException(nameof(Call), request.CallId);
                }

                var streamsSpecification = new StreamsGetFromCallSpecification(request.CallId);
                var streams = await _streamRepository.GetItemsAsync(streamsSpecification);

                if (!streams.Any())
                {
                    throw new EntityNotFoundException($"No injection stream was found for call {request.CallId}");
                }

                var service = await _serviceRepository.GetItemAsync(call.ServiceId);
                if (service == null)
                {
                    throw new EntityNotFoundException(nameof(Service), call.ServiceId);
                }

                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);

                var command = new DoHideInjection.DoHideInjectionCommand
                {
                    CallId = request.CallId,
                };

                await _botServiceClient.HideInjectionAsync(command);

                return null;
            }
        }
    }
}
