using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Cart.API.DTOs;
using EShoppingZone.Cart.API.Services;

namespace EShoppingZone.Cart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;
        
        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }
        
        [HttpGet("{cartId}")]
        public async Task<IActionResult> GetCartById(int cartId)
        {
            var cart = await _cartService.GetCartByIdAsync(cartId);
            if (cart == null)
                return NotFound(new { error = "Cart not found" });
                
            return Ok(cart);
        }
        
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCartByUserId(int userId)
        {
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
                return NotFound(new { error = "Cart not found for this user" });
                
            return Ok(cart);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllCarts()
        {
            var carts = await _cartService.GetAllCartsAsync();
            return Ok(carts);
        }
        
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            if (addToCartDto.Quantity <= 0)
                return BadRequest(new { error = "Quantity must be greater than 0" });
                
            try
            {
                var cart = await _cartService.AddToCartAsync(addToCartDto);
                return Ok(new { message = "Item added to cart successfully", cart });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, new { error = "Failed to add item to cart" });
            }
        }
        
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto updateCartItemDto)
        {
            try
            {
                var cart = await _cartService.UpdateCartItemAsync(updateCartItemDto);
                var message = updateCartItemDto.Quantity <= 0 
                    ? "Item removed from cart" 
                    : "Cart updated successfully";
                    
                return Ok(new { message, cart });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart");
                return StatusCode(500, new { error = "Failed to update cart" });
            }
        }
        
        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(int cartItemId)
        {
            var result = await _cartService.RemoveCartItemAsync(cartItemId);
            if (!result)
                return NotFound(new { error = "Cart item not found" });
                
            return Ok(new { message = "Item removed from cart successfully" });
        }
        
        [HttpDelete("clear/{cartId}")]
        public async Task<IActionResult> ClearCart(int cartId)
        {
            var result = await _cartService.ClearCartAsync(cartId);
            if (!result)
                return NotFound(new { error = "Cart not found" });
                
            return Ok(new { message = "Cart cleared successfully" });
        }
        
        [HttpDelete("{cartId}")]
        public async Task<IActionResult> DeleteCart(int cartId)
        {
            var result = await _cartService.DeleteCartAsync(cartId);
            if (!result)
                return NotFound(new { error = "Cart not found" });
                
            return Ok(new { message = "Cart deleted successfully" });
        }
        
        [HttpGet("{cartId}/total")]
        public async Task<IActionResult> GetCartTotal(int cartId)
        {
            try
            {
                var total = await _cartService.GetCartTotalAsync(cartId);
                return Ok(total);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Cart not found" });
            }
        }
        
        [HttpPost("{cartId}/calculate")]
        public async Task<IActionResult> CalculateAndCacheTotal(int cartId)
        {
            try
            {
                var total = await _cartService.GetCartTotalAsync(cartId);
                return Ok(new 
                { 
                    message = "Cart total calculated", 
                    total = total.TotalPrice,
                    items = total.TotalItems,
                    breakdown = total.Breakdown
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Cart not found" });
            }
        }
    }
}