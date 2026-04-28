
using EShoppingZone.Profile.API.Entities;

namespace EShoppingZone.Profile.API.Repositories
{
    public interface IProfileRepository
    {
        Task<UserProfile?> GetByIdAsync(int id);
        Task<UserProfile?> GetByEmailAsync(string email);
        Task<UserProfile?> GetByMobileNumberAsync(string mobileNumber);
        Task<UserProfile?> GetByFullNameAsync(string fullName);
        Task<UserProfile?> GetByGoogleIdAsync(string googleId);
        Task<IEnumerable<UserProfile>> GetAllAsync();
        Task<(IEnumerable<UserProfile> Users, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<UserProfile>> GetByRoleAsync(string role);
        Task<UserProfile> CreateAsync(UserProfile user);
        Task<UserProfile> UpdateAsync(UserProfile user);
        Task<bool> DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> MobileNumberExistsAsync(string mobileNumber);
        
        Task<Address> AddAddressAsync(Address address);
        Task<Address?> GetAddressByIdAsync(int id);
        Task<IEnumerable<Address>> GetAddressesByUserIdAsync(int userId);
        Task<Address> UpdateAddressAsync(Address address);
        Task<bool> DeleteAddressAsync(int id);
        Task SetDefaultAddressAsync(int userId, int addressId);
    }
}