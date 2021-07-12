// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Application.Common.Models;
using AutoMapper;

namespace Application.Stream
{
    /// <summary>
    ///     Mapping Profile for AutoMapper.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Starting
            CreateMap<StartStreamInjectionBody, Domain.Entities.Stream>()
                .ForPath(
                    dest => dest.Details.StreamUrl,
                    opts => opts.MapFrom(
                        src => src.StreamUrl))
                .ForPath(
                    dest => dest.Details.StreamKey,
                    opts => opts.MapFrom(
                        src => src.StreamKey))
                .ForPath(
                    dest => dest.Details.Protocol,
                    opts => opts.MapFrom(
                        src => src.Protocol))
                .ForPath(
                    dest => dest.Details.Latency,
                    opts =>
                    {
                        opts.Condition(src => (src.Source.Protocol == Domain.Enums.Protocol.SRT));
                        opts.MapFrom(src => (src as SrtStreamInjectionBody).Latency);
                    })
                .ForPath(
                    dest => dest.Details.EnableSsl,
                    opts =>
                    {
                        opts.Condition(src => (src.Source.Protocol == Domain.Enums.Protocol.RTMP));
                        opts.MapFrom(src => (src as RtmpStreamInjectionBody).EnableSsl);
                    })
                .ForPath(
                    dest => dest.Details.Mode,
                    opts => opts.MapFrom(src => src.Protocol == Domain.Enums.Protocol.RTMP
                        ? (dynamic)(src as RtmpStreamInjectionBody).Mode
                        : (dynamic)(src as SrtStreamInjectionBody).Mode));

            CreateMap<Domain.Entities.Stream, StreamModel>()
                .ForPath(
                    dest => dest.InjectionUrl,
                    opts => opts.MapFrom(
                        src => src.Details.StreamUrl))
                .ForPath(
                    dest => dest.Protocol,
                    opts => opts.MapFrom(
                        src => src.Details.Protocol))
                .ForPath(
                    dest => dest.StreamMode,
                    opts => opts.MapFrom(
                        src => src.Details.Mode))
                .ForPath(
                    dest => dest.Passphrase,
                    opts => opts.MapFrom(
                        src => src.Details.StreamKey))
                .ForPath(
                    dest => dest.Latency,
                    opts => opts.MapFrom(
                        src => src.Details.Latency));
        }
    }
}
