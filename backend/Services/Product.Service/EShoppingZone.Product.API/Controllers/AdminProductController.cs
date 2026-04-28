using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Product.API.Services;
using EShoppingZone.Product.API.DTOs;

namespace EShoppingZone.Product.API.Controllers
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")]
    public class AdminProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAdmin([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? search = null)
        {
            var products = await _productService.GetAllProductsForAdminAsync(page, size, search);
            return Ok(products);
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> ToggleStatus(int id, [FromQuery] bool active)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { error = "Product not found" });

            var updateDto = new UpdateProductDto { IsActive = active };
            await _productService.UpdateProductAsync(id, updateDto);
            
            return Ok(new { message = $"Product {(active ? "activated" : "deactivated")} successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePermanently(int id)
        {
            // Note: Our DeleteProductAsync already tries to hard-delete if it's inactive.
            // For Admin, we can force a permanent delete if needed, but we'll use the service logic.
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return BadRequest(new { error = "Product cannot be permanently removed due to existing orders or other constraints." });

            return Ok(new { message = "Product deleted successfully" });
        }
    }
}
