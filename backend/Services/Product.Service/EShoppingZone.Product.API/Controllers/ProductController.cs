using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Product.API.DTOs;
using EShoppingZone.Product.API.Services;

namespace EShoppingZone.Product.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null)
        {
            var products = await _productService.GetAllProductsAsync(pageNumber, pageSize, category, search);
            return Ok(products);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { error = "Product not found" });
                
            return Ok(product);
        }
        
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductsByCategory(string category)
        {
            var validCategories = new[] { "Electronics", "Books", "Apparel", "Personal Care" };
            
            if (!validCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { error = "Invalid category" });
                
            var products = await _productService.GetProductsByCategoryAsync(category);
            return Ok(products);
        }
        
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetProductsByType(string type)
        {
            var products = await _productService.GetProductsByTypeAsync(type);
            return Ok(products);
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { error = "Search term is required" });
                
            var products = await _productService.GetProductsByNameAsync(name);
            return Ok(products);
        }
        
        [HttpPost("{id}/review")]
        public async Task<IActionResult> AddReview(int id, [FromBody] AddReviewDto reviewDto)
        {
            var result = await _productService.AddReviewAsync(id, reviewDto);
            if (!result)
                return NotFound(new { error = "Product not found" });
                
            return Ok(new { message = "Review added successfully" });
        }
        
        [HttpGet("merchant/{merchantId}")]
        public async Task<IActionResult> GetProductsByMerchant(int merchantId)
        {
            var products = await _productService.GetProductsByMerchantAsync(merchantId);
            return Ok(products);
        }
    }
}   