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
    public class MuteBot
    {
        /// <summary>
        /// 
        /// </summary>
        public class MuteBotCommand: IRequest<MuteBotCommandResponse>
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public class MuteBotCommandResponse
        {

        }

        public class MuteBotCommandHandler : IRequestHandler<MuteBotCommand, MuteBotCommandResponse>
        {
            private readonly IBot bot;

            public MuteBotCommandHandler(IBot bot) 
            {
                this.bot = bot;
            }

            public async Task<MuteBotCommandResponse> Handle(MuteBotCommand request, CancellationToken cancellationToken)
            {
                await bot.MuteBotAsync();
                return null;
            }
        }
    }
}
