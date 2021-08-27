// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Call.Commands;
using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;
using static Application.Service.Commands.DoStopServiceInfrastructure;

namespace Application.Service.Events
{
    public class StopServiceInfrastructureRequested
    {
        public class StopServiceInfrastructureRequestedEvent : INotification
        {
            public string CallId { get; set; }

            public string ServiceId { get; set; }
        }

        public class StopServiceInfrastructureRequestedEventHandler : INotificationHandler<StopServiceInfrastructureRequestedEvent>
        {
            private readonly IMediator _mediator;
            private readonly ICallRepository _callRepository;
            private readonly IAzStorageHandler _storageHandler;
            private readonly ILogger<StopServiceInfrastructureRequestedEventHandler> _logger;
            private readonly List<Domain.Enums.CallState> _activeCallStates = new List<Domain.Enums.CallState> { Domain.Enums.CallState.Establishing, Domain.Enums.CallState.Established };

            public StopServiceInfrastructureRequestedEventHandler(
                IMediator mediator,
                ICallRepository callRepository,
                IAzStorageHandler storageHandler,
                ILogger<StopServiceInfrastructureRequestedEventHandler> logger)
            {
                _mediator = mediator;
                _callRepository = callRepository;
                _storageHandler = storageHandler;
                _logger = logger;
            }

            public async Task Handle(StopServiceInfrastructureRequestedEvent notification, CancellationToken cancellationToken)
            {
                var callId = notification.CallId;

                if (!string.IsNullOrEmpty(callId))
                {
                    var assignedCall = await _callRepository.GetItemAsync(callId);

                    if (assignedCall != null && _activeCallStates.Contains(assignedCall.State))
                    {
                        try
                        {
                            var requestEndCallCommand = new RequestEndCall.RequestEndCallCommand
                            {
                                CallId = callId,
                                ShouldShutDownService = false,
                            };

                            await _mediator.Send(requestEndCallCommand);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not end call with id {id}", callId);
                        }
                    }
                }

                var stopServiceInfrastructureCommand = new DoStopServiceInfrastructureCommand
                {
                    Id = notification.ServiceId,
                };

                await _storageHandler.AddQueueMessageAsync(Constants.AzureQueueNames.StopVirtualMachineQueue, stopServiceInfrastructureCommand);
            }
        }
    }
}
