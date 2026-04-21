using EShoppingZone.Product.API.DTOs;
using EShoppingZone.Product.API.Entities;

namespace EShoppingZone.Product.API.Services
{
    public interface IProductService
    {
        Task<ProductDto> AddProductAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int id);
        Task<ProductDetailDto?> GetProductByIdAsync(int id);
        Task<PaginatedResponseDto<ProductDto>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10, string? category = null, string? searchTerm = null);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<ProductDto>> GetProductsByTypeAsync(string type);
        Task<IEnumerable<ProductDto>> GetProductsByNameAsync(string name);
        Task<IEnumerable<ProductDto>> GetProductsByMerchantAsync(int merchantId);
        Task<bool> AddReviewAsync(int productId, AddReviewDto reviewDto);
        Task<bool> UpdateStockAsync(int productId, int quantity);
    }
}