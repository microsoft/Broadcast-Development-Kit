// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Parts;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Stream.Commands
{
    public class DoStopInjection
    {
        public class DoStopInjectionCommand : IRequest<DoStopInjectionCommandResponse>
        {
            public string CallId { get; set; }

            public string StreamId { get; set; }
        }

        public class DoStopInjectionCommandResponse
        {
            public string Id { get; set; }

            public StreamModel Resource { get; set; }
        }

        public class DoStopInjectionCommandValidator : AbstractValidator<DoStopInjectionCommand>
        {
            public DoStopInjectionCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
                RuleFor(x => x.StreamId)
                   .NotEmpty();
            }
        }

        public class DoStopInjectionCommandHandler : IRequestHandler<DoStopInjectionCommand, DoStopInjectionCommandResponse>
        {
            private readonly IBot _bot;
            private readonly IStreamRepository _streamRepository;
            private readonly IMapper _mapper;

            public DoStopInjectionCommandHandler(
                IBot bot,
                IStreamRepository streamRepository,
                IMapper mapper)
            {
                _bot = bot ?? throw new ArgumentNullException(nameof(bot));
                _streamRepository = streamRepository ?? throw new ArgumentNullException(nameof(streamRepository));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<DoStopInjectionCommandResponse> Handle(DoStopInjectionCommand request, CancellationToken cancellationToken)
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

                DoStopInjectionCommandResponse response = new DoStopInjectionCommandResponse
                {
                    Id = entity.Id,
                    Resource = _mapper.Map<StreamModel>(entity),
                };

                return response;
            }
        }
    }
}
