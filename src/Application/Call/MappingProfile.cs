using AutoMapper;
using Application.Call.Commands;
using Application.Common.Models;

namespace Application.Call
{
    /// <summary>
    ///     Mapping Profile for AutoMapper
    /// </summary>
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            //Create
            CreateMap<RequestInviteBot.RequestInviteBotCommand, Domain.Entities.Call>();

            CreateMap<Domain.Entities.Call, CallModel>();
        }
    }
}
