// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models.Api;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Stream.Commands
{
    public class RequestSetInjectionVolume
    {
        public class RequestSetInjectionVolumeCommand : IRequest<RequestSetInjectionVolumeCommandResponse>
        {
            public StreamVolumeFormat Format { get; set; }

            public double Value { get; set; }

            public string CallId { get; set; }
        }

        public class RequestSetInjectionVolumeCommandResponse
        {
        }

        public class RequestSetInjectionVolumeCommandValidator : AbstractValidator<RequestSetInjectionVolumeCommand>
        {
            public RequestSetInjectionVolumeCommandValidator()
            {
                RuleFor(x => x.Format)
                    .NotNull();
                RuleFor(x => x.Value)
                    .NotNull();
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class RequestSetInjectionVolumeCommandHandler : IRequestHandler<RequestSetInjectionVolumeCommand, RequestSetInjectionVolumeCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;

            public RequestSetInjectionVolumeCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository)
            {
                _botServiceClient = botServiceClient;
                _callRepository = callRepository;
                _serviceRepository = serviceRepository;
            }

            public async Task<RequestSetInjectionVolumeCommandResponse> Handle(RequestSetInjectionVolumeCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);
                var service = await _serviceRepository.GetItemAsync(call.ServiceId);

                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);
                var injectionVolume = new SetInjectionVolumeRequest
                {
                    Value = request.Value,
                    Format = request.Format,
                };

                await _botServiceClient.SetInjectionVolumeAsync(request.CallId, injectionVolume);
                return null;
            }
        }
    }
}
