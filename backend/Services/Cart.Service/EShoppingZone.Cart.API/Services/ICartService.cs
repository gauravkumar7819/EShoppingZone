using EShoppingZone.Cart.API.DTOs;
using EShoppingZone.Cart.API.Entities;
using CartModel =EShoppingZone.Cart.API.Entities.Cart;

namespace EShoppingZone.Cart.API.Services
{
    public interface ICartService
    {
        Task<CartDto?> GetCartByIdAsync(int cartId);
        Task<CartDto?> GetCartByUserIdAsync(int userId);
        Task<IEnumerable<CartDto>> GetAllCartsAsync();
        Task<CartDto> AddToCartAsync(AddToCartDto addToCartDto);
        Task<CartDto> UpdateCartItemAsync(UpdateCartItemDto updateCartItemDto);
        Task<bool> RemoveCartItemAsync(int cartItemId);
        Task<bool> ClearCartAsync(int cartId);
        Task<bool> DeleteCartAsync(int cartId);
        Task<CartTotalResponseDto> GetCartTotalAsync(int cartId);
        decimal CalculateCartTotal(CartModel cart);
    }
}