using AutoMapper;
using EShoppingZone.Profile.API.DTOs;
using EShoppingZone.Profile.API.Entities;

namespace EShoppingZone.Profile.API.Mappings
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<UserProfile, ProfileDto>();
            CreateMap<Address, AddressDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.HouseNumber, opt => opt.MapFrom(src => src.HouseNumber))
                .ForMember(dest => dest.StreetName, opt => opt.MapFrom(src => src.StreetName))
                .ForMember(dest => dest.ColonyName, opt => opt.MapFrom(src => src.ColonyName))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dest => dest.Pincode, opt => opt.MapFrom(src => src.Pincode))
                .ForMember(dest => dest.Landmark, opt => opt.MapFrom(src => src.Landmark))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));
            CreateMap<RegisterDto, UserProfile>();
            CreateMap<UpdateProfileDto, UserProfile>();
            CreateMap<CreateAddressDto, Address>();
        }
    }
}