using EShoppingZone.Profile.API.DTOs;
using EShoppingZone.Profile.API.Entities;

namespace EShoppingZone.Profile.API.Services
{
    public interface IProfileService
    {
        Task<UserProfile> AddCustomerProfileAsync(RegisterDto registerDto);
        Task<UserProfile> AddMerchantProfileAsync(RegisterDto registerDto);
        Task<UserProfile> AddDeliveryAgentAsync(RegisterDto registerDto);
        Task<UserProfile> UpdateProfileAsync(int userId, UpdateProfileDto updateDto);
        Task<bool> DeleteProfileAsync(int userId);
        Task<ProfileDto?> GetProfileByIdAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> VerifyEmailAsync(int userId);
        
        // Address methods
        Task<Address> AddAddressAsync(int userId, CreateAddressDto addressDto);
        Task<IEnumerable<AddressDto>> GetUserAddressesAsync(int userId);
        Task<bool> UpdateAddressAsync(int addressId, CreateAddressDto addressDto);
        Task<bool> DeleteAddressAsync(int addressId);
        Task<bool> SetDefaultAddressAsync(int userId, int addressId);
        
        // Admin methods
        Task<IEnumerable<UserProfile>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<UserProfile?> SuspendUserAsync(int userId);
        Task<UserProfile?> ReactivateUserAsync(int userId);
        Task<int> GetTotalUserCountAsync();
    }
}