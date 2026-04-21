using AutoMapper;
using EShoppingZone.Product.API.DTOs;
using EShoppingZone.Product.API.Entities;
using EShoppingZone.Product.API.Repositories;
using ProductModel = EShoppingZone.Product.API.Entities.Product;

namespace EShoppingZone.Product.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        
        public ProductService(
            IProductRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        
        public async Task<ProductDto> AddProductAsync(CreateProductDto createProductDto)
        {
            var product = _mapper.Map<ProductModel>(createProductDto);
            product.CreatedAt = DateTime.UtcNow;
            product.IsActive = true;
            
            var createdProduct = await _repository.CreateAsync(product);
            return _mapper.Map<ProductDto>(createdProduct);
        }
        
        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return null;
            
            _mapper.Map(updateProductDto, product);
            product.UpdatedAt = DateTime.UtcNow;
            
            var updatedProduct = await _repository.UpdateAsync(product);
            return _mapper.Map<ProductDto>(updatedProduct);
        }
        
        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
        
        public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return null;
            
            var productDetail = _mapper.Map<ProductDetailDto>(product);
            productDetail.Ratings = product.Ratings;
            productDetail.Reviews = product.Reviews;
            productDetail.AverageRating = product.AverageRating;
            productDetail.TotalReviews = product.TotalReviews;
            
            return productDetail;
        }
        
        public async Task<PaginatedResponseDto<ProductDto>> GetAllProductsAsync(
            int pageNumber = 1, int pageSize = 10, string? category = null, string? searchTerm = null)
        {
            var (products, totalCount) = await _repository.GetPaginatedAsync(pageNumber, pageSize, category, searchTerm);
            
            return new PaginatedResponseDto<ProductDto>
            {
                Items = _mapper.Map<List<ProductDto>>(products),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
        {
            var products = await _repository.GetByCategoryAsync(category);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        
        public async Task<IEnumerable<ProductDto>> GetProductsByTypeAsync(string type)
        {
            var products = await _repository.GetByTypeAsync(type);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        
        public async Task<IEnumerable<ProductDto>> GetProductsByNameAsync(string name)
        {
            var products = await _repository.GetByProductNameAsync(name);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        
        public async Task<IEnumerable<ProductDto>> GetProductsByMerchantAsync(int merchantId)
        {
            var products = await _repository.GetByMerchantIdAsync(merchantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        
        public async Task<bool> AddReviewAsync(int productId, AddReviewDto reviewDto)
        {
            return await _repository.AddReviewAsync(productId, reviewDto.UserId, reviewDto.Rating, reviewDto.Review);
        }
        
        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await _repository.GetByIdAsync(productId);
            if (product == null) return false;
            
            if (product.StockQuantity < quantity) return false;
            
            await _repository.UpdateStockAsync(productId, quantity);
            return true;
        }
    }
}