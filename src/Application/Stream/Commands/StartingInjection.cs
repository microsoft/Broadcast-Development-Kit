using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using static Domain.Constants.Constants;
using Domain.Entities;
using System.Collections.Generic;
using Application.Exceptions;

namespace Application.Stream.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class StartingInjection
    {
        /// <summary>
        /// 
        /// </summary>
        public class StartingInjectionCommand : IRequest<StartingInjectionCommandResponse>
        {
            public StartStreamInjectionBody Body { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class StartingInjectionCommandResponse
        {
            public string Id { get; set; }
            public StreamModel Resource { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class StartingInjectionCommandValidator : AbstractValidator<StartingInjectionCommand>
        {
            public StartingInjectionCommandValidator()
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

        /// <summary>
        /// 
        /// </summary>
        public class StartingInjectionCommandHandler : IRequestHandler<StartingInjectionCommand, StartingInjectionCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IStreamRepository _streamRepository;
            private readonly IMapper _mapper;

            public StartingInjectionCommandHandler(
                IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository,
                IStreamRepository streamRepository,
                IMapper mapper
                )
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
                _streamRepository = streamRepository ?? throw new ArgumentNullException(nameof(streamRepository));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<StartingInjectionCommandResponse> Handle(StartingInjectionCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.Body.CallId);
                request.Body.StreamKey = GetStreamKeyByProtocol(request.Body, call.PrivateContext);

                StartingInjectionCommandResponse response = new StartingInjectionCommandResponse();

                var entity = _mapper.Map<Domain.Entities.Stream>(request.Body);
                entity.StartingAt = DateTime.UtcNow;
                entity.State = StreamState.Starting;

                await _streamRepository.AddItemAsync(entity);

                request.Body.StreamId = entity.Id;

                var command = new StartInjection.StartInjectionCommand
                {
                    Body = request.Body
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

            private string GetStreamKeyByProtocol(StartStreamInjectionBody startStreamInjectionBody, Dictionary<string, string> privateCallContext)
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
