// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Call.Commands
{
    public class SetCallContext
    {
        public class SetCallContextCommand : IRequest<SetCallContextResponse>
        {
            public string CallId { get; set; }

            public ContextPrivacy PrivacyLevel { get; set; }

            public Dictionary<string, string> Values { get; set; }
        }

        public class SetCallContextResponse
        {
        }

        public class SetCallContextCommandHandler : IRequestHandler<SetCallContextCommand, SetCallContextResponse>
        {
            private readonly ICallRepository _callRepository;

            public SetCallContextCommandHandler(ICallRepository callRepository)
            {
                _callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
            }

            public async Task<SetCallContextResponse> Handle(SetCallContextCommand request, CancellationToken cancellationToken)
            {
                var call = await _callRepository.GetItemAsync(request.CallId);

                switch (request.PrivacyLevel)
                {
                    case ContextPrivacy.Private:
                        UpdateContext(call.PrivateContext, request.Values);
                        break;
                    case ContextPrivacy.Public:
                        UpdateContext(call.PublicContext, request.Values);
                        break;
                }

                await _callRepository.UpdateItemAsync(call.Id, call);

                return new SetCallContextResponse();
            }

            private static void UpdateContext(Dictionary<string, string> callContext, Dictionary<string, string> newContext)
            {
                // Note that if a key-value pair is not included in the newContext, we don't remove it from the callContext.
                foreach (var keyValuePair in newContext)
                {
                    // If the value is null or whitespace, we simply delete the entry from the current context.
                    if (string.IsNullOrWhiteSpace(keyValuePair.Value))
                    {
                        if (callContext.ContainsKey(keyValuePair.Key))
                        {
                            callContext.Remove(keyValuePair.Key);
                        }
                    }
                    else
                    {
                        callContext[keyValuePair.Key] = keyValuePair.Value;
                    }
                }
            }
        }
    }
}
