using Application.Interfaces.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class UnmuteBot
    {
        /// <summary>
        /// 
        /// </summary>
        public class UnmuteBotCommand: IRequest<UnmuteBotCommandResponse>
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public class UnmuteBotCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        public class UnmuteBotCommandHandler : IRequestHandler<UnmuteBotCommand, UnmuteBotCommandResponse>
        {
            private readonly IBot bot;

            public UnmuteBotCommandHandler(IBot bot) 
            {
                this.bot = bot;
            }

            public async Task<UnmuteBotCommandResponse> Handle(UnmuteBotCommand request, CancellationToken cancellationToken)
            {
                var response = new UnmuteBotCommandResponse();
                
                await bot.UnmuteBotAsync();

                return response;
            }
        }
    }
}
