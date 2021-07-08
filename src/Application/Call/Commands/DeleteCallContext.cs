// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Domain.Enums;
using MediatR;

namespace Application.Call.Commands
{
    public class DeleteCallContext
    {
        public class DeleteCallContextCommand : IRequest<DeleteCallContextResponse>
        {
            public string CallId { get; set; }

            public ContextPrivacy PrivacyLevel { get; set; }
        }

        public class DeleteCallContextResponse
        {
        }

        public class DeleteCallContextHandler : IRequestHandler<DeleteCallContextCommand, DeleteCallContextResponse>
        {
            private readonly ICallRepository _callRepository;

            public DeleteCallContextHandler(ICallRepository callRepository)
            {
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
            }

            public async Task<DeleteCallContextResponse> Handle(DeleteCallContextCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);

                switch (request.PrivacyLevel)
                {
                    case ContextPrivacy.Private:
                        call.PrivateContext = new Dictionary<string, string>();
                        break;
                    case ContextPrivacy.Public:
                        call.PublicContext = new Dictionary<string, string>();
                        break;
                }

                await _callRepository.UpdateItemAsync(call.Id, call);

                return new DeleteCallContextResponse();
            }
        }
    }
}
