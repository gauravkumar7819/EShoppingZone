using System.Text.Json;
using EShoppingZone.Order.API.DTOs;

namespace EShoppingZone.Order.API.Integrations
{
    public class ProfileServiceClient : IProfileServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProfileServiceClient> _logger;
        
        public ProfileServiceClient(HttpClient httpClient, ILogger<ProfileServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        
        public async Task<AddressResponseDto?> GetAddressByIdAsync(int addressId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Profile/address/{addressId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AddressResponseDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogWarning("Failed to get address {AddressId}: {StatusCode}", addressId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address {AddressId}", addressId);
                return null;
            }
        }
        
        public async Task<string?> GetFormattedAddressAsync(int addressId)
        {
            var address = await GetAddressByIdAsync(addressId);
            if (address == null) return null;
            
            return $"{address.HouseNumber}, {address.StreetName}, {address.ColonyName}, {address.City}, {address.State} - {address.Pincode}";
        }
        
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/admin/users");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<IEnumerable<UserResponseDto>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Items ?? new List<UserResponseDto>();
                }
                
                return new List<UserResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<UserResponseDto>();
            }
        }
        
        public async Task<UserResponseDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Profile/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserResponseDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return null;
            }
        }
        
        public async Task<bool> SuspendUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"/api/admin/users/{userId}/suspend", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending user {UserId}", userId);
                return false;
            }
        }
        
        public async Task<bool> ReactivateUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"/api/admin/users/{userId}/reactivate", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user {UserId}", userId);
                return false;
            }
        }
    }
    
    public class ApiResponse<T>
    {
        public T? Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}