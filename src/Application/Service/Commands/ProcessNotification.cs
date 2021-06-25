using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using MediatR;

namespace Application.Service.Commands
{
    public class ProcessNotification
    {
        public class ProcessNotificationCommand : IRequest<ProcessNotificationCommandResponse>
        {
            public HttpRequestMessage HttpRequestMessage { get; set; }
        }

        public class ProcessNotificationCommandResponse
        {
            public string Id { get; set; }
        }

        public class ProcessNotificationCommandHandler : IRequestHandler<ProcessNotificationCommand, ProcessNotificationCommandResponse>
        {
            private readonly IBot _bot;

            public ProcessNotificationCommandHandler(IBot bot)
            {
                _bot = bot;
            }

            public async Task<ProcessNotificationCommandResponse> Handle(ProcessNotificationCommand request, CancellationToken cancellationToken)
            {
                var response = new ProcessNotificationCommandResponse();

                await _bot.ProcessNotificationAsync(request.HttpRequestMessage);

                return response;
            }
        }
    }
}
