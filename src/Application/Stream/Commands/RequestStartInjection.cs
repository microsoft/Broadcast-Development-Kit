// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Application.Stream.Specifications;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Parts;
using Domain.Enums;
using FluentValidation;
using MediatR;
using static Domain.Constants.Constants;

namespace Application.Stream.Commands
{
    public class RequestStartInjection
    {
        public class RequestStartInjectionCommand : IRequest<RequestStartInjectionCommandResponse>
        {
            public StartStreamInjectionBody Body { get; set; }
        }

        public class RequestStartInjectionCommandResponse
        {
            public string Id { get; set; }

            public StreamModel Resource { get; set; }
        }

        public class RequestStartInjectionCommandValidator : AbstractValidator<RequestStartInjectionCommand>
        {
            public RequestStartInjectionCommandValidator()
            {
                RuleFor(x => x.Body.CallId)
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

        public class RequestStartInjectionCommandHandler : IRequestHandler<RequestStartInjectionCommand, RequestStartInjectionCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IStreamRepository _streamRepository;
            private readonly IMapper _mapper;

            public RequestStartInjectionCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository,
                IStreamRepository streamRepository,
                IMapper mapper)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
                _streamRepository = streamRepository ?? throw new ArgumentNullException(nameof(streamRepository));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<RequestStartInjectionCommandResponse> Handle(RequestStartInjectionCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.Body.CallId);
                request.Body.StreamKey = GetStreamKeyByProtocol(request.Body, call.PrivateContext);

                RequestStartInjectionCommandResponse response = new RequestStartInjectionCommandResponse();

                var entity = _mapper.Map<Domain.Entities.Stream>(request.Body);
                entity.StartingAt = DateTime.UtcNow;
                entity.State = StreamState.Starting;
                entity.Details.VideoFeedOn = true;

                var streamsSpecification = new StreamsGetFromCallSpecification(request.Body.CallId);
                var streams = await _streamRepository.GetItemsAsync(streamsSpecification);

                var stream = streams.FirstOrDefault();

                if (stream != null)
                {
                    entity.Id = stream.Id;
                    await _streamRepository.UpdateItemAsync(entity.Id, entity);
                }
                else
                {
                    await _streamRepository.AddItemAsync(entity);
                }

                request.Body.StreamId = entity.Id;

                var command = new DoStartInjection.DoStartInjectionCommand
                {
                    Body = request.Body,
                };

                var service = await _serviceRepository.GetItemAsync(call.ServiceId);

                _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);

                try
                {
                    var botServiceResponse = await _botServiceClient.StartInjectionAsync(command);

                    response.Id = entity.Id;
                    response.Resource = botServiceResponse.Resource;

                    return response;
                }
                catch (Exception)
                {
                    entity.State = StreamState.Disconnected;
                    entity.Error = new StreamErrorDetails(StreamErrorType.StartInjection, Messages.StartInjection.Error);

                    await _streamRepository.UpdateItemAsync(entity.Id, entity);
                    throw;
                }
            }

            private static string GetStreamKeyFromPrivateCallContext(Dictionary<string, string> privateCallContext)
            {
                if (!privateCallContext.TryGetValue("streamKey", out string streamKey))
                {
                    throw new StartStreamInjectionException("Stream key is not configured for this call, RTMP injection in push mode could not be initiated");
                }

                return streamKey;
            }

            private static string GetStreamKeyByProtocol(StartStreamInjectionBody startStreamInjectionBody, Dictionary<string, string> privateCallContext)
            {
                if (startStreamInjectionBody.Protocol == Protocol.RTMP)
                {
                    var rtmpStartStreamInjectionBody = startStreamInjectionBody as RtmpStreamInjectionBody;

                    return rtmpStartStreamInjectionBody.Mode == RtmpMode.Push ?
                        GetStreamKeyFromPrivateCallContext(privateCallContext) :
                        rtmpStartStreamInjectionBody.StreamKey;
                }

                return startStreamInjectionBody.StreamKey;
            }
        }
    }
}
