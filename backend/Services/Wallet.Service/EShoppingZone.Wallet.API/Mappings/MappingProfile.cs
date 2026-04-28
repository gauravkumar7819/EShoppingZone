using AutoMapper;
using EShoppingZone.Wallet.API.DTOs;
using EShoppingZone.Wallet.API.Entities;

namespace EShoppingZone.Wallet.API.Mappings
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<EWallet, WalletDto>();
            CreateMap<Statement, StatementDto>()
                .ForMember(dest => dest.TransactionType, 
                    opt => opt.MapFrom(src => src.TransactionType.ToString()));
        }
    }
}