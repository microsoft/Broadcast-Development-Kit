using Application.Interfaces.Common;
using Application.Interfaces.Persistance;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class InviteBot
    {
        /// <summary>
        /// 
        /// </summary>
        public class InviteBotCommand: IRequest<InviteBotCommandResponse>
        {
            public string MeetingUrl { get; set; }

            public string MeetingId { get; set; }

            public string CallId { get; set; }
        }

        /// <summary>
        ///     Command Response
        /// </summary>
        public class InviteBotCommandResponse
        {
            /// <summary>
            ///     Item Id
            /// </summary>
            public string Id { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class InviteBotCommandValidator: AbstractValidator<InviteBotCommand>
        {
            public InviteBotCommandValidator()
            {
                RuleFor(x => x.MeetingUrl)
                    .NotEmpty();
                RuleFor(x => x.CallId)
                    .NotEmpty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class InviteBotCommandHandler: IRequestHandler<InviteBotCommand, InviteBotCommandResponse>
        {
            private readonly IBot bot;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="bot"></param>
            /// <param name="logger"></param>
            public InviteBotCommandHandler(IBot bot)
            {
                this.bot = bot;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<InviteBotCommandResponse> Handle(InviteBotCommand request, CancellationToken cancellationToken)
            {
                InviteBotCommandResponse response = new InviteBotCommandResponse();

                await bot.InviteBotAsync(request);

                //TODO: Change response
                response.Id = request.CallId;

                return response;
            }
        }
    }
}
