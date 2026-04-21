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
            CreateMap<Address, AddressDto>();
            CreateMap<RegisterDto, UserProfile>();
            CreateMap<UpdateProfileDto, UserProfile>();
            CreateMap<CreateAddressDto, Address>();
        }
    }
}