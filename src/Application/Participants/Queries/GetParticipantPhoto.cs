using FluentValidation;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Common;

namespace Application.Participants.Queries
{
    public class GetParticipantPhoto
    {
        /// <summary>
        ///     Model to Get an entity
        /// </summary>
        public class GetParticipantPhotoQuery : IRequest<GetParticipantPhotoQueryResponse>
        {
            /// <summary>
            ///     Id
            /// </summary>
            public string ParticipantAadId { get; set; }

        }

        /// <summary>
        ///     Query Response
        /// </summary>
        public class GetParticipantPhotoQueryResponse
        {
            /// <summary>
            ///     Participant Photo Stream
            /// </summary>
            public System.IO.Stream Photo { get; set; }
        }

        /// <summary>
        ///     Register Validation 
        /// </summary>
        public class GetParticipantPhotoQueryValidator : AbstractValidator<GetParticipantPhotoQuery>
        {
            /// <summary>
            ///     Validator ctor
            /// </summary>
            public GetParticipantPhotoQueryValidator()
            {
                RuleFor(x => x.ParticipantAadId)
                    .NotEmpty();
            }

        }


        /// <summary>
        ///     Handler
        /// </summary>
        public class GetParticipantPhotoQueryHandler : IRequestHandler<GetParticipantPhotoQuery, GetParticipantPhotoQueryResponse>
        {
            private readonly IGraphService graphService;

           
            /// <summary>
            /// 
            /// </summary>
            /// <param name="graphService"></param>
            /// <param name="logger"></param>
            public GetParticipantPhotoQueryHandler(IGraphService graphService)
            {
                this.graphService = graphService;
            }

            /// <summary>
            ///     Handle
            /// </summary>
            /// <param name="query"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<GetParticipantPhotoQueryResponse> Handle(GetParticipantPhotoQuery request, CancellationToken cancellationToken)
            {
                GetParticipantPhotoQueryResponse response = new GetParticipantPhotoQueryResponse();

                var photo = await graphService.GetParticipantPhotoAsync(request.ParticipantAadId);

                response.Photo = photo;

                return response;
            }
        }
    }
}
