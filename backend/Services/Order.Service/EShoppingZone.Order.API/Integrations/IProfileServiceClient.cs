using EShoppingZone.Order.API.DTOs;

namespace EShoppingZone.Order.API.Integrations
{
    public interface IProfileServiceClient
    {
        Task<AddressResponseDto?> GetAddressByIdAsync(int addressId);
        Task<string?> GetFormattedAddressAsync(int addressId);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(int userId);
        Task<bool> SuspendUserAsync(int userId);
        Task<bool> ReactivateUserAsync(int userId);
    }
    
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}