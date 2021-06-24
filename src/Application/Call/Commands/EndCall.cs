using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Constants;
using Domain.Enums;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Call.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class EndCall
    {
        /// <summary>
        /// 
        /// </summary>
        public class EndCallCommand : IRequest<EndCallCommandResponse>
        {
            public string CallId { get; set; }
            public bool ShouldShutDownService { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class EndCallCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }

            public CallModel Resource { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class EndCallCommandValidator : AbstractValidator<EndCallCommand>
        {
            public EndCallCommandValidator()
            {
                //TODO: Check how to do a custom validation for Meeting URL
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class EndCallCommandHandler : IRequestHandler<EndCallCommand, EndCallCommandResponse>
        {
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IBotServiceClient _botServiceClient;
            private readonly IMapper _mapper;

            public EndCallCommandHandler(ICallRepository callRepository,
                IServiceRepository serviceRepository, 
                IBotServiceClient botServiceClient,
                IMapper mapper)
            {
                _callRepository = callRepository ?? throw new System.ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new System.ArgumentNullException(nameof(serviceRepository));
                _botServiceClient = botServiceClient ?? throw new System.ArgumentNullException(nameof(botServiceClient));
                _mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            }

            public async Task<EndCallCommandResponse> Handle(EndCallCommand request, CancellationToken cancellationToken)
            {
                EndCallCommandResponse response = new EndCallCommandResponse();

                Domain.Entities.Call entity = await _callRepository.GetItemAsync(request.CallId);
                entity.State = CallState.Terminating;

                await _callRepository.UpdateItemAsync(entity.Id, entity);

                //Getting service base url
                var service = await _serviceRepository.GetItemAsync(entity.ServiceId);

                response.Resource = _mapper.Map<CallModel>(entity);
                response.Id = entity.Id;

                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);
                await _botServiceClient.RemoveBotAsync(entity.GraphId);

                return response;
            }
        }
    }
}
