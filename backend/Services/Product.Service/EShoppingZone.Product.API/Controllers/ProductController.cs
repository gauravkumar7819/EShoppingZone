using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Product.API.DTOs;
using EShoppingZone.Product.API.Services;

namespace EShoppingZone.Product.API.Controllers
{
    /// <summary>
    /// Public read-only product endpoints accessible to all users.
    /// Write operations (POST/PUT/DELETE) are restricted to Admin only.
    /// Merchants must use /api/merchant/products which enforces ownership checks.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        
        // ------------------------------------------------------------------ //
        //  Public read endpoints                                               //
        // ------------------------------------------------------------------ //

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
        
        [HttpGet("merchant/{merchantId}")]
        public async Task<IActionResult> GetProductsByMerchant(int merchantId)
        {
            var products = await _productService.GetProductsByMerchantAsync(merchantId);
            return Ok(products);
        }
        
        // ------------------------------------------------------------------ //
        //  Review endpoint – authenticated users can add reviews.             //
        // ------------------------------------------------------------------ //
        
        [HttpPost("{id}/review")]
        [Authorize]
        public async Task<IActionResult> AddReview(int id, [FromBody] AddReviewDto reviewDto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                              ?? User.FindFirst("sub")
                              ?? User.FindFirst("nameid");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            // Enforce: user can only post as themselves
            reviewDto.UserId = userId;

            var result = await _productService.AddReviewAsync(id, reviewDto);
            if (!result)
                return NotFound(new { error = "Product not found" });
                
            return Ok(new { message = "Review added successfully" });
        }
        
        // ------------------------------------------------------------------ //
        //  Admin-only write endpoints                                          //
        //  NOTE: Merchants must use /api/merchant/products instead.           //
        // ------------------------------------------------------------------ //
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                var product = await _productService.AddProductAsync(createProductDto);
                return Ok(new { message = "Product created successfully", product });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            var product = await _productService.UpdateProductAsync(id, updateProductDto);
            if (product == null)
                return NotFound(new { error = "Product not found" });
                
            return Ok(new { message = "Product updated successfully", product });
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound(new { error = "Product not found" });
                
            return Ok(new { message = "Product deleted successfully" });
        }
        
        [HttpPut("{id}/stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, [FromQuery] int quantity)
        {
            var result = await _productService.UpdateStockAsync(id, quantity);
            if (!result)
                return BadRequest(new { error = "Insufficient stock or product not found" });
                
            return Ok(new { message = "Stock updated successfully" });
        }
    }
}