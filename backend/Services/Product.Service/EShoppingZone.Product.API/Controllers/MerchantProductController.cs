using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Product.API.DTOs;
using EShoppingZone.Product.API.Services;

namespace EShoppingZone.Product.API.Controllers
{
    [ApiController]
    [Route("api/merchant/products")]
    public class MerchantProductController : ControllerBase
    {
        private readonly IProductService _productService;
        
        public MerchantProductController(IProductService productService)
        {
            _productService = productService;
        }
        
        [HttpPost]
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
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            var product = await _productService.UpdateProductAsync(id, updateProductDto);
            if (product == null)
                return NotFound(new { error = "Product not found" });
                
            return Ok(new { message = "Product updated successfully", product });
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound(new { error = "Product not found" });
                
            return Ok(new { message = "Product deleted successfully" });
        }
        
        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromQuery] int quantity)
        {
            var result = await _productService.UpdateStockAsync(id, quantity);
            if (!result)
                return BadRequest(new { error = "Insufficient stock or product not found" });
                
            return Ok(new { message = "Stock updated successfully" });
        }
    }
}