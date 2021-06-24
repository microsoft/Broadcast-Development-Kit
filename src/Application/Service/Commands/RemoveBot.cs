using Application.Interfaces.Common;
using FluentValidation;
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
    public class RemoveBot
    {
        /// <summary>
        /// 
        /// </summary>
        public class RemoveBotCommand: IRequest<RemoveBotCommandResponse>
        {
            public string GraphCallId { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class RemoveBotCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class RemoveBotCommandValidator: AbstractValidator<RemoveBotCommand>
        {
            public RemoveBotCommandValidator()
            {
                RuleFor(x => x.GraphCallId)
                    .NotEmpty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class RemoveBotCommandHandler: IRequestHandler<RemoveBotCommand, RemoveBotCommandResponse>
        {
            private readonly IBot bot;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="bot"></param>
            public RemoveBotCommandHandler(IBot bot)
            {
                this.bot = bot;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<RemoveBotCommandResponse> Handle(RemoveBotCommand request, CancellationToken cancellationToken)
            {
                RemoveBotCommandResponse response = new RemoveBotCommandResponse();

                await bot.RemoveBotAsync(request.GraphCallId);

                response.Id = request.GraphCallId;

                return response;
            }
        }
    }
}
