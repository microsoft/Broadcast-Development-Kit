// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Call.Commands
{
    public class RequestEndCall
    {
        public class RequestEndCallCommand : IRequest<RequestEndCallCommandResponse>
        {
            public string CallId { get; set; }

            public bool ShouldShutDownService { get; set; }
        }

        public class RequestEndCallCommandResponse
        {
            public string Id { get; set; }

            public CallModel Resource { get; set; }
        }

        public class RequestEndCallCommandValidator : AbstractValidator<RequestEndCallCommand>
        {
            public RequestEndCallCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class RequestEndCallCommandHandler : IRequestHandler<RequestEndCallCommand, RequestEndCallCommandResponse>
        {
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IBotServiceClient _botServiceClient;
            private readonly IMapper _mapper;

            public RequestEndCallCommandHandler(
                ICallRepository callRepository,
                IServiceRepository serviceRepository,
                IBotServiceClient botServiceClient,
                IMapper mapper)
            {
                _callRepository = callRepository ?? throw new System.ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new System.ArgumentNullException(nameof(serviceRepository));
                _botServiceClient = botServiceClient ?? throw new System.ArgumentNullException(nameof(botServiceClient));
                _mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            }

            public async Task<RequestEndCallCommandResponse> Handle(RequestEndCallCommand request, CancellationToken cancellationToken)
            {
                var response = new RequestEndCallCommandResponse();

                var entity = await _callRepository.GetItemAsync(request.CallId);

                if (entity == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Call), request.CallId);
                }

                entity.State = CallState.Terminating;

                await _callRepository.UpdateItemAsync(entity.Id, entity);

                // Getting service base url
                var service = await _serviceRepository.GetItemAsync(entity.ServiceId);

                if (service == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Service), entity.ServiceId);
                }

                response.Resource = _mapper.Map<CallModel>(entity);
                response.Id = entity.Id;

                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);
                await _botServiceClient.RemoveBotAsync(entity.GraphId);

                return response;
            }
        }
    }
}
