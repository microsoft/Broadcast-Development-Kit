using Application.Interfaces.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessNotification
    {
        public class ProcessNotificationCommand: IRequest<ProcessNotificationCommandResponse>
        {
            public HttpRequestMessage HttpRequestMessage { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class ProcessNotificationCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ProcessNotificationCommandHandler : IRequestHandler<ProcessNotificationCommand, ProcessNotificationCommandResponse>
        {
            private readonly IBot bot;

            public ProcessNotificationCommandHandler(IBot bot)
            {
                this.bot = bot;
            }

            public async Task<ProcessNotificationCommandResponse> Handle(ProcessNotificationCommand request, CancellationToken cancellationToken)
            {
                var response = new ProcessNotificationCommandResponse();

                await bot.ProcessNotificationAsync(request.HttpRequestMessage);

                return response;
            }
        }
    }
}
