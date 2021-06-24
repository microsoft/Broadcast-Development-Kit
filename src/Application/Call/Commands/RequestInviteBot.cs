using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Config;
using Application.Common.Models;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using AutoMapper;
using BotService.Infrastructure.Common;
using Domain.Constants;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Management.Compute.Fluent;
using static Application.Service.Commands.InviteBot;

namespace Application.Call.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class RequestInviteBot
    {
        /// <summary>
        /// 
        /// </summary>
        public class RequestInviteBotCommand : IRequest<RequestInviteBotCommandResponse>
        {
            public string MeetingUrl { get; set; }

            public string MeetingId { get; set; }

            public string ServiceId { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class RequestInviteBotCommandResponse
        {
            //TODO: Modify response
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
            public CallModel Resource { get; set; }

            
        }

        /// <summary>
        /// 
        /// </summary>
        public class RequestInviteBotCommandValidator : AbstractValidator<RequestInviteBotCommand>
        {
            public RequestInviteBotCommandValidator()
            {
                //TODO: Check how to do a custom validation for Meeting URL
                RuleFor(x => x.MeetingUrl)
                    .NotEmpty();
                RuleFor(x => x.MeetingUrl)
                    .Custom((meetingUrl, context) =>
                    {
                        var decodedUrl = WebUtility.UrlDecode(meetingUrl);
                        var regex = new Regex("https://teams\\.microsoft\\.com.*/(?<thread>[^/]+)/(?<message>[^/]+)\\?context=(?<context>{.*})");
                        var match = regex.Match(decodedUrl);
                        if (!match.Success)
                        {
                            context.AddFailure("MeetingUrl cannot be parsed");
                        }
                    });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class RequestInviteBotCommandHandler : IRequestHandler<RequestInviteBotCommand, RequestInviteBotCommandResponse>
        {
            private readonly IBotServiceClient _botServiceClient;
            private readonly ICallRepository _callRepository;
            private readonly IServiceRepository _serviceRepository;
            private readonly IMapper _mapper;
            private readonly IMeetingUrlHelper _meetingUrlHelper;
            private readonly IStreamKeyGeneratorHelper _streamKeyGeneratorHelper;

            /// <summary>
            ///  Ctor
            /// </summary>
            /// <param name="callRepository"></param>
            /// <param name="storageHandler"></param>
            public RequestInviteBotCommandHandler(IBotServiceClient botServiceClient,
                ICallRepository callRepository,
                IServiceRepository serviceRepository,
                IMapper mapper,
                IMeetingUrlHelper meetingUrlHelper,
                IStreamKeyGeneratorHelper streamKeyGeneratorHelper)
            {
                _botServiceClient = botServiceClient ?? throw new ArgumentNullException(nameof(botServiceClient));
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
                _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _meetingUrlHelper = meetingUrlHelper ?? throw new ArgumentNullException(nameof(meetingUrlHelper));
                _streamKeyGeneratorHelper = streamKeyGeneratorHelper ?? throw new ArgumentNullException(nameof(streamKeyGeneratorHelper));
            }

            public async Task<RequestInviteBotCommandResponse> Handle(RequestInviteBotCommand request, CancellationToken cancellationToken)
            {
                RequestInviteBotCommandResponse response = new RequestInviteBotCommandResponse();
                if (string.IsNullOrEmpty(request.MeetingId))
                {
                    _meetingUrlHelper.Init(request.MeetingUrl);
                    request.MeetingId = _meetingUrlHelper.GetMeetingId();
                }
                /* TODO: Change this.
                    NOTE: The Management Portal does not have the feature to select the service before initializing the call.
                    The following code is temporary, if the service Id is not specified, we use a harcoded ID to retrieve the service.
                */

                var serviceId = string.IsNullOrEmpty(request.ServiceId) ? Constants.EnvironmentDefaults.ServiceId : request.ServiceId;
                var service = await _serviceRepository.GetItemAsync(serviceId);

                if (service == null)
                {
                    throw new EntityNotFoundException(nameof(Domain.Entities.Service), serviceId);
                }

                if (service.State != ServiceState.Available)
                {
                    throw new ServiceUnavailableException($"The service {service.Name} is not available");
                }

                if (service.Infrastructure.PowerState != PowerState.Running.Value && service.Infrastructure.ProvisioningDetails.State != ProvisioningStateType.Provisioned)
                {
                    throw new ServiceUnavailableException($"The service {service.Name} is not running");
                }

                Domain.Entities.Call call = _mapper.Map<Domain.Entities.Call>(request);
                call.State = CallState.Establishing;
                call.ServiceId = service.Id;
                call.BotFqdn = service.Infrastructure.Dns;
                call.PrivateContext.Add("streamKey", _streamKeyGeneratorHelper.GetNewStreamKey());
                call.CreatedAt = DateTime.Now;

                await _callRepository.AddItemAsync(call);

                service.CallId = call.Id;
                service.State = ServiceState.Unavailable;

                await _serviceRepository.UpdateItemAsync(service.Id, service);

                var inviteBotCommand = new InviteBotCommand
                {
                    CallId = call.Id,
                    MeetingId = request.MeetingId,
                    MeetingUrl = request.MeetingUrl
                };

                try
                {
                    //TODO: Handle response for error handling
                    _botServiceClient.SetBaseUrl(service.Infrastructure.Dns);
                    await _botServiceClient.InviteBotAsync(inviteBotCommand);

                    response.Id = call.Id;
                    response.Resource = _mapper.Map<CallModel>(call);
                    return response;
                }
                catch (Exception)
                {
                    service.State = ServiceState.Available;
                    await _serviceRepository.UpdateItemAsync(service.Id, service);

                    call.State = CallState.Error;
                    //TODO: Call Error details
                    await _callRepository.UpdateItemAsync(call.Id, call);

                    throw;
                }
            }
        }
    }
}
