using EShoppingZone.Product.API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductModel = EShoppingZone.Product.API.Entities.Product;

namespace EShoppingZone.Product.API.Repositories
{
    public interface IProductRepository
    {
        Task<ProductModel?> GetByIdAsync(int id);
        Task<IEnumerable<ProductModel>> GetAllAsync();
        Task<IEnumerable<ProductModel>> GetByCategoryAsync(string category);
        Task<IEnumerable<ProductModel>> GetByTypeAsync(string type);
        Task<IEnumerable<ProductModel>> GetByProductNameAsync(string name);
        Task<IEnumerable<ProductModel>> GetByMerchantIdAsync(int merchantId);
        Task<IEnumerable<ProductModel>> GetActiveProductsAsync();
        Task<ProductModel> CreateAsync(ProductModel product);
        Task<ProductModel> UpdateAsync(ProductModel product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<(IEnumerable<ProductModel> Products, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize, string? category = null, string? searchTerm = null);
        Task<bool> AddReviewAsync(int productId, int userId, double rating, string review);
        Task UpdateStockAsync(int productId, int quantity);
    }
}