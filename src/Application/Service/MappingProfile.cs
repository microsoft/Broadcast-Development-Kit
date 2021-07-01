using Application.Common.Models;
using AutoMapper;
using static Application.Service.Commands.StartingServiceInfrastructure;
using static Application.Service.Commands.StartServiceInfrastructure;
using static Application.Service.Commands.StoppingServiceInfrastructure;
using static Application.Service.Commands.StopServiceInfrastructure;

namespace Application.Service
{
    /// <summary>
    ///     Mapping Profile for AutoMapper.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Domain.Entities.Service, ServiceModel>();
            CreateMap<Domain.Entities.Parts.Infrastructure, InfrastructureModel>();
            CreateMap<Domain.Entities.Parts.ProvisioningDetails, ProvisioningDetailsModel>();
            CreateMap<Domain.Entities.Service, StartServiceInfrastructureCommand>();
            CreateMap<Domain.Entities.Service, StopServiceInfrastructureCommand>();
            CreateMap<Domain.Entities.Service, StartingServiceInfrastructureCommandResponse>();
            CreateMap<Domain.Entities.Service, StoppingServiceInfrastructureCommandResponse>();
        }
    }
}
