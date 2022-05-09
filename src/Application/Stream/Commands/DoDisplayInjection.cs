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
    public class DoDisplayInjection
    {
        public class DoDisplayInjectionCommand : IRequest<DoDisplayInjectionCommandResponse>
        {
            public string CallId { get; set; }
        }

        public class DoDisplayInjectionCommandResponse
        {
        }

        public class DoDisplayInjectionCommandValidator : AbstractValidator<DoDisplayInjectionCommand>
        {
            public DoDisplayInjectionCommandValidator()
            {
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class DoDisplayInjectionCommandHandler : IRequestHandler<DoDisplayInjectionCommand, DoDisplayInjectionCommandResponse>
        {
            private readonly IBot _bot;
            private readonly ICallRepository _callRepository;
            private readonly IStreamRepository _streamRepository;

            public DoDisplayInjectionCommandHandler(
                IBot bot,
                ICallRepository callRepository,
                IStreamRepository streamRepository)
            {
                _bot = bot ?? throw new ArgumentNullException(nameof(bot));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _streamRepository = streamRepository ?? throw new ArgumentNullException(nameof(streamRepository));
            }

            public async Task<DoDisplayInjectionCommandResponse> Handle(DoDisplayInjectionCommand request, CancellationToken cancellationToken)
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

                _bot.DisplayInjection();

                stream.Details.VideoFeedOn = true;

                await _streamRepository.UpdateItemAsync(stream.Id, stream);

                return null;
            }
        }
    }
}
