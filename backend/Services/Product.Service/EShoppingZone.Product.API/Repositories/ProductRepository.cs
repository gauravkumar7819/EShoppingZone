using Microsoft.EntityFrameworkCore;
using EShoppingZone.Product.API.Data;
using EShoppingZone.Product.API.Entities;
using ProductModel = EShoppingZone.Product.API.Entities.Product;

namespace EShoppingZone.Product.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;
        
        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }
        
        public async Task<ProductModel?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }
        
        public async Task<IEnumerable<ProductModel>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }
        
        public async Task<IEnumerable<ProductModel>> GetByCategoryAsync(string category)
        {
            return await _context.Products
                .Where(p => p.Category.ToLower() == category.ToLower() && p.IsActive)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<ProductModel>> GetByTypeAsync(string type)
        {
            return await _context.Products
                .Where(p => p.Type.ToLower() == type.ToLower() && p.IsActive)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<ProductModel>> GetByProductNameAsync(string name)
        {
            return await _context.Products
                .Where(p => p.Name.ToLower().Contains(name.ToLower()) && p.IsActive)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<ProductModel>> GetByMerchantIdAsync(int merchantId)
        {
            return await _context.Products
                .Where(p => p.MerchantId == merchantId)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<ProductModel>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();
        }
        
        public async Task<ProductModel> CreateAsync(ProductModel product)
        {
            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        
        public async Task<ProductModel> UpdateAsync(ProductModel product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return product;
        }
        
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product == null) return false;
            
            // If the product is already inactive (soft-deleted), 
            // we attempt a physical delete to remove it from the merchant's view.
            if (!product.IsActive)
            {
                try 
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception)
                {
                    // Fallback: If it cannot be hard-deleted (e.g. database constraints like existing orders),
                    // we keep it as inactive.
                    return false;
                }
            }
            
            // Soft delete on first attempt
            product.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }
        
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Products.CountAsync(p => p.IsActive);
        }
        
        public async Task<(IEnumerable<ProductModel> Products, int TotalCount)> GetPaginatedAsync(
            int pageNumber, int pageSize, string? category = null, string? searchTerm = null)
        {
            var query = _context.Products.Where(p => p.IsActive);
            
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.ToLower() == category.ToLower());
            }
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()) || 
                                        p.Description.ToLower().Contains(searchTerm.ToLower()));
            }
            
            var totalCount = await query.CountAsync();
            
            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (products, totalCount);
        }

        public async Task<(IEnumerable<ProductModel> Products, int TotalCount)> GetPaginatedAdminAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.Products.AsQueryable();
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()) || 
                                        p.Description.ToLower().Contains(searchTerm.ToLower()));
            }
            
            var totalCount = await query.CountAsync();
            
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (products, totalCount);
        }
        
        public async Task<bool> AddReviewAsync(int productId, int userId, double rating, string review)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return false;
            
            var ratings = product.Ratings;
            var reviews = product.Reviews;
            
            ratings[userId] = rating;
            reviews[userId] = review;
            
            product.Ratings = ratings;
            product.Reviews = reviews;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task UpdateStockAsync(int productId, int quantity)
        {
            var product = await GetByIdAsync(productId);
            if (product != null)
            {
                product.StockQuantity -= quantity;
                await _context.SaveChangesAsync();
            }
        }
    }
}