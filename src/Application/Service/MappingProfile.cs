// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Application.Common.Models;
using AutoMapper;
using static Application.Service.Commands.DoStartServiceInfrastructure;
using static Application.Service.Commands.DoStopServiceInfrastructure;
using static Application.Service.Commands.RequestStartServiceInfrastructure;
using static Application.Service.Commands.RequestStopServiceInfrastructure;

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
            CreateMap<Domain.Entities.Service, DoStartServiceInfrastructureCommand>();
            CreateMap<Domain.Entities.Service, DoStopServiceInfrastructureCommand>();
            CreateMap<Domain.Entities.Service, RequestStartServiceInfrastructureCommandResponse>();
            CreateMap<Domain.Entities.Service, RequestStopServiceInfrastructureCommandResponse>();
        }
    }
}
