using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Stream.Commands
{
    public class StopInjection
    {
        /// <summary>
        /// 
        /// </summary>
        public class StopInjectionCommand : IRequest<StopInjectionCommandResponse>
        {
            public string CallId { get; set; }
            public string StreamId { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class StopInjectionCommandResponse
        {
            public string Id { get; set; }
            public StreamModel Resource { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class StopInjectionCommandValidator : AbstractValidator<StopInjectionCommand>
        {
            public StopInjectionCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
                RuleFor(x => x.StreamId)
                   .NotEmpty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class StopInjectionCommandHandler : IRequestHandler<StopInjectionCommand, StopInjectionCommandResponse>
        {
            private readonly IBot _bot;
            private readonly IStreamRepository _streamRepository;
            private readonly IMapper _mapper;

            public StopInjectionCommandHandler(
                IBot bot,
                IStreamRepository streamRepository,
                IMapper mapper
                )
            {
                _bot = bot ?? throw new ArgumentNullException(nameof(bot));
                _streamRepository = streamRepository ?? throw new ArgumentNullException(nameof(streamRepository));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<StopInjectionCommandResponse> Handle(StopInjectionCommand request, CancellationToken cancellationToken)
            {
                var entity = await _streamRepository.GetItemAsync(request.StreamId);

                try
                {
                    _bot.StopInjection();

                    entity.Error = null;
                    entity.EndedAt = DateTime.UtcNow;
                    entity.State = StreamState.Disconnected;

                    await _streamRepository.UpdateItemAsync(entity.Id, entity);
                }
                catch (Exception ex)
                {
                    entity.State = StreamState.Disconnected;
                    entity.Error = new StreamErrorDetails(StreamErrorType.StopInjection, ex.Message);

                    await _streamRepository.UpdateItemAsync(entity.Id, entity);

                    throw;
                }

                StopInjectionCommandResponse response = new StopInjectionCommandResponse
                {
                    Id = entity.Id,
                    Resource = _mapper.Map<StreamModel>(entity)
                };

                return response;
            }
        }
    }
}
