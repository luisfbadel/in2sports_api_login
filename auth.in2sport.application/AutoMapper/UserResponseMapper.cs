using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using AutoMapper;
using auth.in2sport.application.Services.UserServices.Response;

namespace auth.in2sport.application.AutoMapper
{
    public class UserResponseMapper : Profile
    {
        public UserResponseMapper() {
            CreateMap<Users, UserResponse>()
                .ForMember(
                    dest => dest.Id,
                    src => src.MapFrom(x => x.Id))
                .ForMember(
                    dest => dest.Email,
                    src => src.MapFrom(x => x.Email))
                .ForMember(
                    dest => dest.TypeUser,
                    src => src.MapFrom(x => x.TypeUser))
                .ForMember(
                    dest => dest.FirstName,
                    src => src.MapFrom(x => x.FirstName))
                .ForMember(
                    dest => dest.SecondName,
                    src => src.MapFrom(x => x.SecondName))
                .ForMember(
                    dest => dest.FirstLastname,
                    src => src.MapFrom(x => x.FirstLastname))
                .ForMember(
                    dest => dest.SecondLastname,
                    src => src.MapFrom(x => x.SecondLastname))
                .ForMember(
                    dest => dest.TypeDocument,
                    src => src.MapFrom(x => x.TypeDocument))
                .ForMember(
                    dest => dest.DocumentNumber,
                    src => src.MapFrom(x => x.DocumentNumber))
                .ForMember(
                    dest => dest.PhoneNumber,
                    src => src.MapFrom(x => x.PhoneNumber))
                .ForMember(
                    dest => dest.Address,
                    src => src.MapFrom(x => x.Address));

        }
    }
}
