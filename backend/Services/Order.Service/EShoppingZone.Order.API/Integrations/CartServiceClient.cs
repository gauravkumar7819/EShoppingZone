using System.Text.Json;
using EShoppingZone.Order.API.DTOs;

namespace EShoppingZone.Order.API.Integrations
{
    public class CartServiceClient : ICartServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CartServiceClient> _logger;
        
        public CartServiceClient(HttpClient httpClient, ILogger<CartServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        
        public async Task<CartResponseDto?> GetCartByIdAsync(int cartId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Cart/{cartId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<CartResponseDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogWarning("Failed to get cart {CartId}: {StatusCode}", cartId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart {CartId}", cartId);
                return null;
            }
        }
        
        public async Task<bool> ClearCartAsync(int cartId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/Cart/clear/{cartId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart {CartId}", cartId);
                return false;
            }
        }
    }
}