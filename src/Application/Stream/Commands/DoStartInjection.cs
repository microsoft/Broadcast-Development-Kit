﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Config;
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
    public class DoStartInjection
    {
        public class DoStartInjectionCommand : IRequest<DoStartInjectionCommandResponse>
        {
            public StartStreamInjectionBody Body { get; set; }
        }

        public class DoStartInjectionCommandResponse
        {
            public string Id { get; set; }

            public StreamModel Resource { get; set; }
        }

        public class DoStartInjectionCommandValidator : AbstractValidator<DoStartInjectionCommand>
        {
            public DoStartInjectionCommandValidator()
            {
                RuleFor(x => x.Body.CallId)
                    .NotEmpty();
                RuleFor(x => x.Body.StreamId)
                    .NotEmpty();
                RuleFor(x => x.Body.Protocol)
                    .NotNull()
                    .Must(p => p == Protocol.RTMP || p == Protocol.SRT)
                    .WithMessage("Protocol not supported for media injection");
                When(p => p.Body.Protocol == Protocol.RTMP, () =>
                {
                    RuleFor(x => ((RtmpStreamInjectionBody)x.Body).Mode)
                        .NotNull()
                        .Must(p => p == RtmpMode.Pull || p == RtmpMode.Push)
                        .WithMessage("RTMP Mode not supported for media injection");
                    When(x => ((RtmpStreamInjectionBody)x.Body).Mode == RtmpMode.Pull, () =>
                    {
                        RuleFor(x => x.Body.StreamUrl)
                            .NotEmpty();
                    });
                });
                When(p => p.Body.Protocol == Protocol.SRT, () =>
                {
                    RuleFor(x => ((SrtStreamInjectionBody)x.Body).Mode)
                        .NotNull()
                        .Must(p => p == SrtMode.Listener || p == SrtMode.Caller)
                        .WithMessage("SRT Mode not supported for media injection");
                    When(x => ((SrtStreamInjectionBody)x.Body).Mode == SrtMode.Caller, () =>
                    {
                        RuleFor(x => x.Body.StreamUrl)
                            .NotEmpty();
                    });
                });
            }
        }

        public class DoStartInjectionCommandHandler : IRequestHandler<DoStartInjectionCommand, DoStartInjectionCommandResponse>
        {
            private readonly IAppConfiguration _configuration;
            private readonly IBot _bot;
            private readonly IStreamRepository _streamRepository;
            private readonly IInjectionUrlHelper _injectionUrlHelper;
            private readonly IMapper _mapper;

            public DoStartInjectionCommandHandler(
                IAppConfiguration configuration,
                IBot bot,
                IStreamRepository streamRepository,
                IInjectionUrlHelper injectionUrlHelper,
                IMapper mapper)
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _bot = bot ?? throw new ArgumentNullException(nameof(bot));
                _streamRepository = streamRepository ?? throw new ArgumentNullException(nameof(streamRepository));
                _injectionUrlHelper = injectionUrlHelper ?? throw new ArgumentNullException(nameof(injectionUrlHelper));
                _mapper = mapper;
            }

            public async Task<DoStartInjectionCommandResponse> Handle(DoStartInjectionCommand request, CancellationToken cancellationToken)
            {
                var entity = await _streamRepository.GetItemAsync(request.Body.StreamId);

                try
                {
                    _bot.StartInjection(request.Body);

                    entity.Error = null;
                    entity.Details.StreamUrl = _injectionUrlHelper.GetStreamUrl(request.Body, _configuration.BotConfiguration.ServiceDnsName);
                    entity.StartedAt = DateTime.UtcNow;
                    entity.State = StreamState.Ready;
                    entity.Details.StreamVolume = new StreamVolume
                    {
                        Value = request.Body.StreamVolume.Value,
                        Format = request.Body.StreamVolume.Format,
                    };

                    await _streamRepository.UpdateItemAsync(entity.Id, entity);
                }
                catch (Exception ex)
                {
                    entity.State = StreamState.Disconnected;
                    entity.Error = new StreamErrorDetails(StreamErrorType.StartInjection, ex.Message);

                    await _streamRepository.UpdateItemAsync(entity.Id, entity);

                    throw;
                }

                DoStartInjectionCommandResponse response = new DoStartInjectionCommandResponse
                {
                    Id = entity.Id,
                    Resource = _mapper.Map<StreamModel>(entity),
                };

                return response;
            }
        }
    }
}
