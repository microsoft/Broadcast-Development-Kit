using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Domain.Entities;
using Domain.Entities.Parts;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using static Domain.Constants.Constants;

namespace Application.Stream.Commands
{
    public class StoppingInjection
    {
        public class StoppingInjectionCommand : IRequest<StoppingInjectionCommandResponse>
        {
            public string CallId { get; set; }

            public string StreamId { get; set; }
        }

        public class StoppingInjectionCommandResponse
        {
            public string Id { get; set; }

            public StreamModel Resource { get; set; }
        }

        public class StoppingInjectionCommandValidator : AbstractValidator<StoppingInjectionCommand>
        {
            public StoppingInjectionCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
                RuleFor(x => x.StreamId)
                    .NotEmpty();
            }
        }

        public class StoppingInjectionCommandHandler : IRequestHandler<StoppingInjectionCommand, StoppingInjectionCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IStreamRepository _streamRepository;

            public StoppingInjectionCommandHandler(
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

            public async Task<StoppingInjectionCommandResponse> Handle(StoppingInjectionCommand request, CancellationToken cancellationToken)
            {
                StoppingInjectionCommandResponse response = new StoppingInjectionCommandResponse();

                var call = await _callRepository.GetItemAsync(request.CallId);
                var service = await _serviceRepository.GetItemAsync(call.ServiceId);
                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);

                var entity = await _streamRepository.GetItemAsync(request.StreamId);

                if (entity == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Stream), request.StreamId);
                }

                entity.EndingAt = DateTime.UtcNow;
                entity.State = StreamState.Stopping;

                await _streamRepository.UpdateItemAsync(entity.Id, entity);

                var command = new StopInjection.StopInjectionCommand
                {
                    CallId = request.CallId,
                    StreamId = entity.Id,
                };

                try
                {
                    var botServiceResponse = await _botServiceClient.StoptInjectionAsync(command);

                    response.Id = entity.Id;
                    response.Resource = botServiceResponse.Resource;

                    return response;
                }
                catch (Exception)
                {
                    entity.State = StreamState.Disconnected;
                    entity.Error = new StreamErrorDetails(StreamErrorType.StopInjection, Messages.StopInjection.Error);

                    await _streamRepository.UpdateItemAsync(entity.Id, entity);

                    throw;
                }
            }
        }
    }
}
