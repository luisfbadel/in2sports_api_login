using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using AutoMapper;
using auth.in2sport.application.Services.UserServices.Response;

namespace auth.in2sport.application.AutoMapper
{
    public class User : Profile
    {
        public User() {
            CreateMap<Users, UserResponse>()
                .ForMember(
                    dest => dest.Id,
                    src => src.MapFrom(x => x.Id))
                .ForMember(
                    dest => dest.Email,
                    src => src.MapFrom(x => x.Email))
                .ForMember(
                    dest => dest.status,
                    src => src.MapFrom(x => x.status));

        }
    }
}
