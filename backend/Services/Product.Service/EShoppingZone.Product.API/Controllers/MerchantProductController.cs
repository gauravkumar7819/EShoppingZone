using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Product.API.DTOs;
using EShoppingZone.Product.API.Services;
using System.Security.Claims;

namespace EShoppingZone.Product.API.Controllers
{
    /// <summary>
    /// Merchant-scoped product management. Enforces ownership and identity via JWT.
    /// Route changed to api/merchant-products to avoid conflict with other merchant-related routes.
    /// </summary>
    [ApiController]
    [Route("api/merchant-products")]
    [Authorize(Roles = "Merchant,Admin")]
    public class MerchantProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public MerchantProductController(IProductService productService)
        {
            _productService = productService;
        }

        private int? GetAuthenticatedUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("nameid");

            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;

            return null;
        }

        private bool IsAdmin() => User.IsInRole("Admin");

        // GET: api/merchant-products/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyProducts()
        {
            var merchantId = GetAuthenticatedUserId();
            if (merchantId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            var products = await _productService.GetProductsByMerchantAsync(merchantId.Value);
            return Ok(products);
        }

        // POST: api/merchant-products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            var merchantId = GetAuthenticatedUserId();
            if (merchantId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            createProductDto.MerchantId = merchantId.Value;

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

        // PUT: api/merchant-products/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            var authUserId = GetAuthenticatedUserId();
            if (authUserId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            var existing = await _productService.GetProductByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "Product not found." });

            if (!IsAdmin() && existing.MerchantId != authUserId.Value)
                return StatusCode(403, new { error = "Unauthorized access to this product." });

            var updated = await _productService.UpdateProductAsync(id, updateProductDto);
            return Ok(new { message = "Product updated successfully", product = updated });
        }

        // DELETE: api/merchant-products/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var authUserId = GetAuthenticatedUserId();
            if (authUserId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            var existing = await _productService.GetProductByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "Product not found." });

            if (!IsAdmin() && existing.MerchantId != authUserId.Value)
                return StatusCode(403, new { error = "Unauthorized access to this product." });

            var result = await _productService.DeleteProductAsync(id);
            if (!result)
            {
                   return BadRequest(new { 
                       error = "Product cannot be permanently removed because it has order history. It will remain as 'Inactive' to preserve sales records." 
                   });
            }
            return Ok(new { message = "Product removed successfully" });
        }

        // PUT: api/merchant-products/{id}/stock
        [HttpPut("{id:int}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromQuery] int quantity)
        {
            var authUserId = GetAuthenticatedUserId();
            if (authUserId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            var existing = await _productService.GetProductByIdAsync(id);
            if (existing == null || (!IsAdmin() && existing.MerchantId != authUserId.Value))
                return StatusCode(403, new { error = "Unauthorized access." });

            var result = await _productService.UpdateStockAsync(id, quantity);
            if (!result)
                return BadRequest(new { error = "Insufficient stock." });

            return Ok(new { message = "Stock updated successfully" });
        }
    }
}