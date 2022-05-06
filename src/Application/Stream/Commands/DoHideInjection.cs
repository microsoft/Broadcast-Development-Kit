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
    public class DoHideInjection
    {
        public class DoHideInjectionCommand : IRequest<DoHideInjectionCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class DoHideInjectionCommandResponse
        {
            public string Id { get; set; }
        }

        public class DoHideInjectionCommandValidator : AbstractValidator<DoHideInjectionCommand>
        {
            public DoHideInjectionCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class DoHideInjectionCommandHandler : IRequestHandler<DoHideInjectionCommand, DoHideInjectionCommandResponse>
        {
            private readonly IBot _bot;
            private readonly ICallRepository _callRepository;
            private readonly IStreamRepository _streamRepository;

            public DoHideInjectionCommandHandler(
                IBot bot,
                ICallRepository callRepository,
                IStreamRepository streamRepository)
            {
                _bot = bot ?? throw new ArgumentNullException(nameof(bot));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _streamRepository = streamRepository ?? throw new ArgumentNullException(nameof(streamRepository));
            }

            public async Task<DoHideInjectionCommandResponse> Handle(DoHideInjectionCommand request, CancellationToken cancellationToken)
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

                var stream = streams.First();

                _bot.HideInjection();

                stream.Details.VideoFeedOn = false;

                await _streamRepository.UpdateItemAsync(stream.Id, stream);

                return null;
            }
        }
    }
}
