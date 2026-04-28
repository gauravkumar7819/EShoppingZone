using EShoppingZone.Order.API.DTOs;

namespace EShoppingZone.Order.API.Integrations
{
    public interface ICartServiceClient
    {
        Task<CartResponseDto?> GetCartByIdAsync(int cartId);
        Task<bool> ClearCartAsync(int cartId);
    }
}