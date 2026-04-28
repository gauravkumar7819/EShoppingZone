using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using EShoppingZone.Cart.API.DTOs;
using EShoppingZone.Cart.API.Entities;
using EShoppingZone.Cart.API.Repositories;
using CartModel =EShoppingZone.Cart.API.Entities.Cart;

namespace EShoppingZone.Cart.API.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repository;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CartService> _logger;
        
        public CartService(
            ICartRepository repository,
            IMapper mapper,
            IDistributedCache cache,
            ILogger<CartService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }
        
        public decimal CalculateCartTotal(CartModel cart)
        {
            if (cart?.Items == null) return 0;
            return cart.Items.Sum(i => i.Price * i.Quantity);
        }
        
        private async Task<CartDto?> GetCachedCartAsync(int cartId)
        {
            var cacheKey = $"cart_{cartId}";
            var cachedCart = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedCart))
            {
                return JsonSerializer.Deserialize<CartDto>(cachedCart);
            }
            
            return null;
        }
        
        private async Task SetCachedCartAsync(CartDto cartDto)
        {
            var cacheKey = $"cart_{cartDto.CartId}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cartDto), options);
        }
        
        private async Task RemoveCachedCartAsync(int cartId)
        {
            var cacheKey = $"cart_{cartId}";
            await _cache.RemoveAsync(cacheKey);
        }
        
        public async Task<CartDto?> GetCartByIdAsync(int cartId)
        {
            // Try to get from cache first
            var cachedCart = await GetCachedCartAsync(cartId);
            if (cachedCart != null)
            {
                _logger.LogInformation("Cart {CartId} retrieved from cache", cartId);
                return cachedCart;
            }
            
            // Get from database
            var cart = await _repository.GetCartByIdAsync(cartId);
            if (cart == null) return null;
            
            var cartDto = _mapper.Map<CartDto>(cart);
            cartDto.TotalPrice = CalculateCartTotal(cart);
            cartDto.TotalItems = cart.Items.Sum(i => i.Quantity);
            
            // Cache the result
            await SetCachedCartAsync(cartDto);
            
            return cartDto;
        }
        
        public async Task<CartDto?> GetCartByUserIdAsync(int userId)
        {
            var cart = await _repository.GetCartByUserIdAsync(userId);
            if (cart == null) return null;
            
            return await GetCartByIdAsync(cart.CartId);
        }
        
        public async Task<IEnumerable<CartDto>> GetAllCartsAsync()
        {
            var carts = await _repository.GetAllCartsAsync();
            var cartDtos = _mapper.Map<IEnumerable<CartDto>>(carts);
            
            foreach (var cartDto in cartDtos)
            {
                var cart = carts.First(c => c.CartId == cartDto.CartId);
                cartDto.TotalPrice = CalculateCartTotal(cart);
                cartDto.TotalItems = cart.Items.Sum(i => i.Quantity);
            }
            
            return cartDtos;
        }
        
        public async Task<CartDto> AddToCartAsync(AddToCartDto addToCartDto)
        {
            // Get or create cart
            var cart = await _repository.GetCartByUserIdAsync(addToCartDto.UserId);
            
            if (cart == null)
            {
                cart = new CartModel
                {
                    UserId = addToCartDto.UserId,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };
                cart = await _repository.CreateCartAsync(cart);
            }
            
            // Check if product already exists in cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == addToCartDto.ProductId);
            
            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += addToCartDto.Quantity;
                await _repository.UpdateCartItemAsync(existingItem);
            }
            else
            {
                // Add new item
                var cartItem = new CartItem
                {
                    ProductId = addToCartDto.ProductId,
                    MerchantId = addToCartDto.MerchantId,
                    ProductName = addToCartDto.ProductName,
                    Price = addToCartDto.Price,
                    Quantity = addToCartDto.Quantity,
                    ImageUrl = addToCartDto.ImageUrl ?? string.Empty,
                    CartId = cart.CartId
                };
                await _repository.AddCartItemAsync(cartItem);
            }
            
            // Update cart timestamp
            cart.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateCartAsync(cart);
            
            // Get updated cart
            var updatedCart = await _repository.GetCartByIdAsync(cart.CartId);
            var cartDto = _mapper.Map<CartDto>(updatedCart);
            cartDto.TotalPrice = CalculateCartTotal(updatedCart!);
            cartDto.TotalItems = updatedCart!.Items.Sum(i => i.Quantity);
            
            // Update cache
            await SetCachedCartAsync(cartDto);
            
            return cartDto;
        }
        
        public async Task<CartDto> UpdateCartItemAsync(UpdateCartItemDto updateCartItemDto)
        {
            var cartItem = await _repository.GetCartItemByIdAsync(updateCartItemDto.CartItemId);
            
            if (cartItem == null)
                throw new KeyNotFoundException("Cart item not found");
            
            if (cartItem.CartId != updateCartItemDto.CartId)
                throw new InvalidOperationException("Cart item does not belong to specified cart");
            
            if (updateCartItemDto.Quantity <= 0)
            {
                // Remove item if quantity is 0 or negative
                await _repository.RemoveCartItemAsync(updateCartItemDto.CartItemId);
            }
            else
            {
                // Update quantity
                cartItem.Quantity = updateCartItemDto.Quantity;
                await _repository.UpdateCartItemAsync(cartItem);
            }
            
            // Get updated cart
            var cart = await _repository.GetCartByIdAsync(updateCartItemDto.CartId);
            if (cart == null)
                throw new KeyNotFoundException("Cart not found");
            
            cart.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateCartAsync(cart);
            
            var cartDto = _mapper.Map<CartDto>(cart);
            cartDto.TotalPrice = CalculateCartTotal(cart);
            cartDto.TotalItems = cart.Items.Sum(i => i.Quantity);
            
            // Update cache
            await SetCachedCartAsync(cartDto);
            
            return cartDto;
        }
        
        public async Task<bool> RemoveCartItemAsync(int cartItemId)
        {
            var cartItem = await _repository.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null) return false;
            
            var cartId = cartItem.CartId;
            var result = await _repository.RemoveCartItemAsync(cartItemId);
            
            if (result)
            {
                // Invalidate cache
                await RemoveCachedCartAsync(cartId);
            }
            
            return result;
        }
        
        public async Task<bool> ClearCartAsync(int cartId)
        {
            var cart = await _repository.GetCartByIdAsync(cartId);
            if (cart == null) return false;
            
            await _repository.ClearCartItemsAsync(cartId);
            cart.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateCartAsync(cart);
            
            // Invalidate cache
            await RemoveCachedCartAsync(cartId);
            
            return true;
        }
        
        public async Task<bool> DeleteCartAsync(int cartId)
        {
            var result = await _repository.DeleteCartAsync(cartId);
            
            if (result)
            {
                // Invalidate cache
                await RemoveCachedCartAsync(cartId);
            }
            
            return result;
        }
        
        public async Task<CartTotalResponseDto> GetCartTotalAsync(int cartId)
        {
            var cart = await _repository.GetCartByIdAsync(cartId);
            if (cart == null)
                throw new KeyNotFoundException("Cart not found");
            
            var total = CalculateCartTotal(cart);
            
            return new CartTotalResponseDto
            {
                CartId = cartId,
                TotalPrice = total,
                TotalItems = cart.Items.Sum(i => i.Quantity),
                Breakdown = cart.Items.Select(i => new CartItemBreakdownDto
                {
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Subtotal = i.Price * i.Quantity
                }).ToList()
            };
        }
    }
}