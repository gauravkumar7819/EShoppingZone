using Microsoft.EntityFrameworkCore;
using EShoppingZone.Profile.API.Data;
using EShoppingZone.Profile.API.Entities;

namespace EShoppingZone.Profile.API.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ProfileDbContext _context;
        
        public ProfileRepository(ProfileDbContext context)
        {
            _context = context;
        }
        
        public async Task<UserProfile?> GetByIdAsync(int id)
        {
            return await _context.UserProfiles
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        
        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            return await _context.UserProfiles
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        
        public async Task<UserProfile?> GetByMobileNumberAsync(string mobileNumber)
        {
            return await _context.UserProfiles
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber);
        }
        
        public async Task<UserProfile?> GetByFullNameAsync(string fullName)
        {
            return await _context.UserProfiles
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.FullName == fullName);
        }
        
        public async Task<UserProfile?> GetByGoogleIdAsync(string googleId)
        {
            return await _context.UserProfiles
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }
        
        public async Task<IEnumerable<UserProfile>> GetAllAsync()
        {
            return await _context.UserProfiles
                .Include(u => u.Addresses)
                .ToListAsync();
        }

        public async Task<(IEnumerable<UserProfile> Users, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.UserProfiles.CountAsync();
            var users = await _context.UserProfiles
                .Include(u => u.Addresses)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return (users, totalCount);
        }
        
        public async Task<IEnumerable<UserProfile>> GetByRoleAsync(string role)
        {
            return await _context.UserProfiles
                .Include(u => u.Addresses)
                .Where(u => u.Role == role)
                .ToListAsync();
        }
        
        public async Task<UserProfile> CreateAsync(UserProfile user)
        {
            user.CreatedAt = DateTime.UtcNow;
            _context.UserProfiles.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        
        public async Task<UserProfile> UpdateAsync(UserProfile user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return user;
        }
        
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;
            
            _context.UserProfiles.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.UserProfiles.AnyAsync(u => u.Email == email);
        }
        
        public async Task<bool> MobileNumberExistsAsync(string mobileNumber)
        {
            return await _context.UserProfiles.AnyAsync(u => u.MobileNumber == mobileNumber);
        }
        
        public async Task<Address> AddAddressAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }
        
        public async Task<Address?> GetAddressByIdAsync(int id)
        {
            return await _context.Addresses
                .Include(a => a.UserProfile)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        
        public async Task<IEnumerable<Address>> GetAddressesByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserProfileId == userId)
                .ToListAsync();
        }
        
        public async Task<Address> UpdateAddressAsync(Address address)
        {
            _context.Entry(address).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return address;
        }
        
        public async Task<bool> DeleteAddressAsync(int id)
        {
            var address = await GetAddressByIdAsync(id);
            if (address == null) return false;
            
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task SetDefaultAddressAsync(int userId, int addressId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserProfileId == userId)
                .ToListAsync();
                
            foreach (var addr in addresses)
            {
                addr.IsDefault = false;
            }
            
            if (addressId > 0)
            {
                var defaultAddress = await GetAddressByIdAsync(addressId);
                if (defaultAddress != null)
                {
                    defaultAddress.IsDefault = true;
                }
            }
            
            await _context.SaveChangesAsync();
        }
    }
}