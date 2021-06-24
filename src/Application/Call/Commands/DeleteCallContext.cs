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
            private readonly ICallRepository callRepository;

            public DeleteCallContextHandler(ICallRepository callRepository)
            {
                this.callRepository = callRepository ?? throw new ArgumentNullException(nameof(callRepository));
            }

            public async Task<DeleteCallContextResponse> Handle(DeleteCallContextCommand request, CancellationToken cancellationToken)
            {
                var call = await callRepository.GetItemAsync(request.CallId);

                switch (request.PrivacyLevel)
                {
                    case ContextPrivacy.Private:
                        call.PrivateContext = new Dictionary<string, string>();
                        break;
                    case ContextPrivacy.Public:
                        call.PublicContext = new Dictionary<string, string>();
                        break;
                }

                await callRepository.UpdateItemAsync(call.Id, call);

                return new DeleteCallContextResponse();
            }
        }
    }
}
