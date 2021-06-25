using Application.Common.Models;
using AutoMapper;
using BotService.Infrastructure.Extensions;
using Domain.Enums;
using Microsoft.Graph.Communications.Calls;
using static Application.Participant.Commands.UpdateParticipantMeetingStatus;

namespace BotService.Application.Participant
{
    /// <summary>
    ///     Mapping Profile for AutoMapper.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Create
            CreateMap<IParticipant, ParticipantStreamModel>()
                .ForMember(dest => dest.Id, act => act.Ignore())
                .ForMember(
                    dest => dest.AadId,
                    opts => opts.MapFrom(
                        src => src.GetUserIdentity().Id))
                .ForMember(
                    dest => dest.ParticipantGraphId,
                    opts => opts.MapFrom(
                        src => src.Id))
                .ForMember(
                    dest => dest.DisplayName,
                    opts => opts.MapFrom(
                        src => src.GetUserIdentity().DisplayName))
                 .ForMember(
                    dest => dest.Type,
                    opts => opts.MapFrom(
                        src => ResourceType.Participant))
                 .ForMember(
                    dest => dest.State,
                    opts => opts.MapFrom(
                        src => StreamState.Disconnected))
                 .ForMember(
                    dest => dest.IsHealthy,
                    opts => opts.MapFrom(
                        src => true))
                 .ForMember(
                    dest => dest.HealthMessage,
                    opts => opts.MapFrom(
                        src => string.Empty))
                 .ForMember(
                    dest => dest.AudioMuted,
                    opts => opts.MapFrom(
                        src => src.Resource.IsMuted.HasValue && src.Resource.IsMuted.Value))
                .ForMember(
                    dest => dest.IsSharingAudio,
                    opts => opts.MapFrom(
                        src => src.IsParticipantCapableToSendAudio()))
                .ForMember(
                    dest => dest.IsSharingScreen,
                    opts => opts.MapFrom(
                        src => src.IsParticipantSharingScreen()))
                .ForMember(
                    dest => dest.IsSharingVideo,
                    opts => opts.MapFrom(
                        src => src.IsParticipantCapableToSendVideo()));

            // Update
            CreateMap<IParticipant, UpdateParticipantMeetingStatusCommand>()
                .ForMember(
                    dest => dest.ParticipantGraphId,
                    opts => opts.MapFrom(
                        src => src.Id))
                .ForMember(
                    dest => dest.AudioMuted,
                    opts => opts.MapFrom(
                        src => src.Resource.IsMuted.HasValue && src.Resource.IsMuted.Value))
                .ForMember(
                    dest => dest.IsSharingAudio,
                    opts => opts.MapFrom(
                        src => src.IsParticipantCapableToSendAudio()))
                .ForMember(
                    dest => dest.IsSharingScreen,
                    opts => opts.MapFrom(
                        src => src.IsParticipantSharingScreen()))
                .ForMember(
                    dest => dest.IsSharingVideo,
                    opts => opts.MapFrom(
                        src => src.IsParticipantCapableToSendVideo()));
        }
    }
}
