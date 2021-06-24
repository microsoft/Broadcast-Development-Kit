using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Application.Service.Specifications;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Service.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class SetBotServiceAsAvailable
    {
        /// <summary>
        /// 
        /// </summary>
        public class SetBotServiceAsAvailableCommand: IRequest<SetBotServiceAsAvailableCommandResponse>
        {
            public string CallId { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class SetBotServiceAsAvailableCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SetBotServiceAsAvailableCommandValidator : AbstractValidator<SetBotServiceAsAvailableCommand>
        {
            public SetBotServiceAsAvailableCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SetBotServiceAsAvailableCommandHandler : IRequestHandler<SetBotServiceAsAvailableCommand, SetBotServiceAsAvailableCommandResponse>
        {
            private readonly IServiceRepository _serviceRepository;
            private readonly ILogger<SetBotServiceAsAvailableCommandHandler> _logger;

            /// <summary>
            ///     Ctor
            /// </summary>
            /// <param name="serviceRepository"></param>
            /// <param name="logger"></param>
            public SetBotServiceAsAvailableCommandHandler(IServiceRepository serviceRepository,
                ILogger<SetBotServiceAsAvailableCommandHandler> logger)
            {
                _serviceRepository = serviceRepository ?? throw new System.ArgumentNullException(nameof(serviceRepository));
                _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="request"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<SetBotServiceAsAvailableCommandResponse> Handle(SetBotServiceAsAvailableCommand request, CancellationToken cancellationToken)
            {
                var response = new SetBotServiceAsAvailableCommandResponse();

                var specification = new ServiceGetByCallIdSpecification(request.CallId);

                var services = await _serviceRepository.GetItemsAsync(specification);
               
                var entity = services.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogError("Bot service associated to call {id} was not found", request.CallId);
                    throw new EntityNotFoundException($"Bot service associated to call {request.CallId} was not found");
                }

                entity.CallId = null;
                entity.State = Domain.Enums.ServiceState.Available;

                await _serviceRepository.UpdateItemAsync(entity.Id, entity);

                response.Id = entity.Id;

                return response;
            }
        }
    }
}
