// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Application.Stream.Specifications;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Stream.Commands
{
    public class UpdateStreamState
    {
        public class UpdateStreamStateCommand : IRequest<UpdateStreamStateCommandResponse>
        {
            public string CallId { get; set; }

            public StreamState StreamState { get; set; }
        }

        public class UpdateStreamStateCommandResponse
        {
            public string Id { get; set; }
        }

        public class UpdateStreamStateCommandHandler : IRequestHandler<UpdateStreamStateCommand, UpdateStreamStateCommandResponse>
        {
            private readonly IStreamRepository _streamRepository;
            private readonly ILogger<UpdateStreamStateCommandHandler> _logger;

            public UpdateStreamStateCommandHandler(
                IStreamRepository streamRepository,
                ILogger<UpdateStreamStateCommandHandler> logger)
            {
                _streamRepository = streamRepository;
                _logger = logger;
            }

            public async Task<UpdateStreamStateCommandResponse> Handle(UpdateStreamStateCommand command, CancellationToken cancellationToken)
            {
                UpdateStreamStateCommandResponse response = new UpdateStreamStateCommandResponse();

                var streamsSpecification = new StreamsGetFromCallSpecification(command.CallId);
                var streams = await _streamRepository.GetItemsAsync(streamsSpecification);
                var stream = streams.FirstOrDefault();

                if (stream == null)
                {
                    _logger.LogInformation("Stream from call {callId} was not found", command.CallId);
                    throw new EntityNotFoundException($"Stream from call {command.CallId} wasn't found");
                }

                stream.State = command.StreamState;
                await _streamRepository.UpdateItemAsync(stream.Id, stream);

                return response;
            }
        }
    }
}
