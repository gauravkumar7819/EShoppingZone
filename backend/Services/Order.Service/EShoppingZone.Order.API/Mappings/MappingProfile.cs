using AutoMapper;
using EShoppingZone.Order.API.DTOs;
using EShoppingZone.Order.API.Entities;
using OrderModel=EShoppingZone.Order.API.Entities.Order;
namespace EShoppingZone.Order.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<OrderModel, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ModeOfPayment, opt => opt.MapFrom(src => src.ModeOfPayment.ToString()));
            
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Price * src.Quantity));
            
            CreateMap<StatusHistory, StatusHistoryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}