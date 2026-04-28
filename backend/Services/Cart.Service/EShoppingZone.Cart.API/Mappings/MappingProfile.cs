using AutoMapper;
using EShoppingZone.Cart.API.DTOs;
using EShoppingZone.Cart.API.Entities;
using CartModel =EShoppingZone.Cart.API.Entities.Cart;
namespace EShoppingZone.Cart.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CartModel, CartDto>()
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
                .ForMember(dest => dest.TotalItems, opt => opt.Ignore());
                
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Price * src.Quantity));
                
            CreateMap<AddToCartDto, CartItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CartId, opt => opt.Ignore())
                .ForMember(dest => dest.Cart, opt => opt.Ignore());
        }
    }
}