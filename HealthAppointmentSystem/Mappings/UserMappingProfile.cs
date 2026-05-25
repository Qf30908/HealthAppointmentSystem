using AutoMapper;
using HealthAppointmentSystem.AUTH;
using HealthAppointmentSystem.AUTH.AuthDTOs;

namespace HealthAppointmentSystem.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, AuthResponseDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        }
    }
}
