// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models.Api;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Application.Stream.Specifications;
using Domain.Entities.Parts;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Stream.Commands
{
    public class DoSetInjectionVolume
    {
        public class DoSetInjectionVolumeCommand : IRequest<DoSetInjectionVolumeCommandResponse>
        {
            public StreamVolumeFormat Format { get; set; }

            public double Value { get; set; }

            public string CallId { get; set; }
        }

        public class DoSetInjectionVolumeCommandResponse
        {
        }

        public class DoSetInjectionVolumeCommandValidator : AbstractValidator<DoSetInjectionVolumeCommand>
        {
            public DoSetInjectionVolumeCommandValidator()
            {
                RuleFor(x => x.Format)
                    .NotNull();
                RuleFor(x => x.Value)
                    .NotNull();
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        public class DoSetInjectionVolumeCommandHandler : IRequestHandler<DoSetInjectionVolumeCommand, DoSetInjectionVolumeCommandResponse>
        {
            private readonly IBot _bot;
            private readonly IStreamRepository _streamRepository;

            public DoSetInjectionVolumeCommandHandler(
                IBot bot,
                IStreamRepository streamRepository)
            {
                _bot = bot;
                _streamRepository = streamRepository;
            }

            public async Task<DoSetInjectionVolumeCommandResponse> Handle(DoSetInjectionVolumeCommand request, CancellationToken cancellationToken)
            {
                var streamsSpecification = new StreamsGetFromCallSpecification(request.CallId);
                var streams = await _streamRepository.GetItemsAsync(streamsSpecification);

                if (!streams.Any())
                {
                    throw new EntityNotFoundException($"No injection stream was found for call {request.CallId}");
                }

                var stream = streams.First();
                if (stream.State != StreamState.Ready && stream.State != StreamState.Receiving && stream.State != StreamState.NotReceiving)
                {
                    throw new SetStreamVolumeException("Set stream volume", $"The injection stream hasn't started. Current state: {Enum.GetName(typeof(StreamState), stream.State)}");
                }

                var injectionVolume = new SetInjectionVolumeRequest
                {
                    Value = request.Value,
                    Format = request.Format,
                };

                _bot.SetInjectionVolume(injectionVolume);

                stream.Details.StreamVolume = new StreamVolume
                {
                    Value = request.Value,
                    Format = request.Format,
                };

                await _streamRepository.UpdateItemAsync(stream.Id, stream);

                return null;
            }
        }
    }
}
