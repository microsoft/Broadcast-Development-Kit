// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using FluentValidation;
using MediatR;

namespace Application.Participants.Queries
{
    public class GetParticipantPhoto
    {
        public class GetParticipantPhotoQuery : IRequest<GetParticipantPhotoQueryResponse>
        {
            public string ParticipantAadId { get; set; }
        }

        public class GetParticipantPhotoQueryResponse
        {
            public System.IO.Stream Photo { get; set; }
        }

        public class GetParticipantPhotoQueryValidator : AbstractValidator<GetParticipantPhotoQuery>
        {
            public GetParticipantPhotoQueryValidator()
            {
                RuleFor(x => x.ParticipantAadId)
                    .NotEmpty();
            }
        }

        public class GetParticipantPhotoQueryHandler : IRequestHandler<GetParticipantPhotoQuery, GetParticipantPhotoQueryResponse>
        {
            private readonly IGraphService _graphService;

            public GetParticipantPhotoQueryHandler(IGraphService graphService)
            {
                _graphService = graphService;
            }

            public async Task<GetParticipantPhotoQueryResponse> Handle(GetParticipantPhotoQuery request, CancellationToken cancellationToken)
            {
                GetParticipantPhotoQueryResponse response = new GetParticipantPhotoQueryResponse();

                var photo = await _graphService.GetParticipantPhotoAsync(request.ParticipantAadId);

                response.Photo = photo;

                return response;
            }
        }
    }
}
