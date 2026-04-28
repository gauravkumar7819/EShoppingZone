using EShoppingZone.Cart.API.Entities;
using CartModel =EShoppingZone.Cart.API.Entities.Cart;

namespace EShoppingZone.Cart.API.Repositories
{
    public interface ICartRepository
    {
        Task<CartModel?> GetCartByIdAsync(int cartId);
        Task<CartModel?> GetCartByUserIdAsync(int userId);
        Task<IEnumerable<CartModel>> GetAllCartsAsync();
        Task<CartModel> CreateCartAsync(CartModel cart);
        Task<CartModel> UpdateCartAsync(CartModel cart);
        Task<bool> DeleteCartAsync(int cartId);
        Task<bool> CartExistsAsync(int cartId);
        Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
        Task<CartItem> AddCartItemAsync(CartItem cartItem);
        Task<CartItem> UpdateCartItemAsync(CartItem cartItem);
        Task<bool> RemoveCartItemAsync(int cartItemId);
        Task ClearCartItemsAsync(int cartId);
    }
}