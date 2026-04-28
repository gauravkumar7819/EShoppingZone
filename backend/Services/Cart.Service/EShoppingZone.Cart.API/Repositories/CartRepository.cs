using Microsoft.EntityFrameworkCore;
using EShoppingZone.Cart.API.Data;
using EShoppingZone.Cart.API.Entities;
using CartModel =EShoppingZone.Cart.API.Entities.Cart;

namespace EShoppingZone.Cart.API.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _context;
        
        public CartRepository(CartDbContext context)
        {
            _context = context;
        }
        
        public async Task<CartModel?> GetCartByIdAsync(int cartId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CartId == cartId);
        }
        
        public async Task<CartModel?> GetCartByUserIdAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }
        
        public async Task<IEnumerable<CartModel>> GetAllCartsAsync()
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ToListAsync();
        }
        
        public async Task<CartModel> CreateCartAsync(CartModel cart)
        {
            cart.CreatedAt = DateTime.UtcNow;
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }
        
        public async Task<CartModel> UpdateCartAsync(CartModel cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Entry(cart).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cart;
        }
        
        public async Task<bool> DeleteCartAsync(int cartId)
        {
            var cart = await GetCartByIdAsync(cartId);
            if (cart == null) return false;
            
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> CartExistsAsync(int cartId)
        {
            return await _context.Carts.AnyAsync(c => c.CartId == cartId);
        }
        
        public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == cartItemId);
        }
        
        public async Task<CartItem> AddCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }
        
        public async Task<CartItem> UpdateCartItemAsync(CartItem cartItem)
        {
            _context.Entry(cartItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cartItem;
        }
        
        public async Task<bool> RemoveCartItemAsync(int cartItemId)
        {
            var cartItem = await GetCartItemByIdAsync(cartItemId);
            if (cartItem == null) return false;
            
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task ClearCartItemsAsync(int cartId)
        {
            var cart = await GetCartByIdAsync(cartId);
            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
            }
        }
    }
}