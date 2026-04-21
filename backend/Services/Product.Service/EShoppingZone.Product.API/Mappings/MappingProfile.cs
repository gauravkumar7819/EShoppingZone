using AutoMapper;
using EShoppingZone.Product.API.DTOs;
using EShoppingZone.Product.API.Entities;
using ProductModel = EShoppingZone.Product.API.Entities.Product;

namespace EShoppingZone.Product.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductModel, ProductDto>()
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.AverageRating))
                .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => src.TotalReviews));
                
            CreateMap<ProductModel, ProductDetailDto>()
                .ForMember(dest => dest.Ratings, opt => opt.MapFrom(src => src.Ratings))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.AverageRating))
                .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => src.TotalReviews));
                
            CreateMap<CreateProductDto, ProductModel>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images ?? new List<string>()))
                .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => src.Specifications ?? new Dictionary<string, string>()));
                
            CreateMap<UpdateProductDto, ProductModel>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}