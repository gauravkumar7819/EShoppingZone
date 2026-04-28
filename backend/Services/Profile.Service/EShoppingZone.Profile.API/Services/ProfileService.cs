using AutoMapper;
using Microsoft.AspNetCore.Identity;
using EShoppingZone.Profile.API.DTOs;
using EShoppingZone.Profile.API.Entities;
using EShoppingZone.Profile.API.Repositories;

namespace EShoppingZone.Profile.API.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _repository;
        private readonly IPasswordHasher<UserProfile> _passwordHasher;
        private readonly IMapper _mapper;
        
        public ProfileService(
            IProfileRepository repository,
            IPasswordHasher<UserProfile> passwordHasher,
            IMapper mapper)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }
        
        private async Task<UserProfile> CreateProfileAsync(RegisterDto registerDto, string role)
        {
            if (await _repository.EmailExistsAsync(registerDto.Email))
                throw new InvalidOperationException("Email already registered");
                
            if (await _repository.MobileNumberExistsAsync(registerDto.MobileNumber))
                throw new InvalidOperationException("Mobile number already registered");
            
            var user = new UserProfile
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                MobileNumber = registerDto.MobileNumber,
                Role = role,
                About = registerDto.About ?? string.Empty,
                DateOfBirth = registerDto.DateOfBirth,
                Gender = registerDto.Gender ?? string.Empty,
                IsActive = true,
                IsEmailVerified = false
            };
            
            user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password);
            
            return await _repository.CreateAsync(user);
        }
        
        public async Task<UserProfile> AddCustomerProfileAsync(RegisterDto registerDto)
        {
            return await CreateProfileAsync(registerDto, "Customer");
        }
        
        public async Task<UserProfile> AddMerchantProfileAsync(RegisterDto registerDto)
        {
            return await CreateProfileAsync(registerDto, "Merchant");
        }
        
        public async Task<UserProfile> AddDeliveryAgentAsync(RegisterDto registerDto)
        {
            return await CreateProfileAsync(registerDto, "DeliveryAgent");
        }
        
        public async Task<UserProfile> UpdateProfileAsync(int userId, UpdateProfileDto updateDto)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");
                
            user.FullName = updateDto.FullName;
            user.About = updateDto.About;
            user.DateOfBirth = updateDto.DateOfBirth;
            user.Gender = updateDto.Gender;
            user.ImageUrl = updateDto.ImageUrl;
            
            return await _repository.UpdateAsync(user);
        }
        
        public async Task<bool> DeleteProfileAsync(int userId)
        {
            return await _repository.DeleteAsync(userId);
        }
        
        public async Task<ProfileDto?> GetProfileByIdAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return null;
            
            var profileDto = _mapper.Map<ProfileDto>(user);
            profileDto.Addresses = _mapper.Map<List<AddressDto>>(user.Addresses);
            
            return profileDto;
        }
        
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");
                
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, changePasswordDto.CurrentPassword);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Current password is incorrect");
                
            user.PasswordHash = _passwordHasher.HashPassword(user, changePasswordDto.NewPassword);
            await _repository.UpdateAsync(user);
            
            return true;
        }
        
        public async Task<bool> VerifyEmailAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return false;
            
            user.IsEmailVerified = true;
            await _repository.UpdateAsync(user);
            return true;
        }
        
        public async Task<Address> AddAddressAsync(int userId, CreateAddressDto addressDto)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");
                
            var address = new Address
            {
                HouseNumber = addressDto.HouseNumber,
                StreetName = addressDto.StreetName,
                ColonyName = addressDto.ColonyName,
                City = addressDto.City,
                State = addressDto.State,
                Pincode = addressDto.Pincode,
                Landmark = addressDto.Landmark,
                IsDefault = addressDto.IsDefault,
                UserProfileId = userId
            };
            
            var existingAddresses = await _repository.GetAddressesByUserIdAsync(userId);
            if (!existingAddresses.Any())
            {
                address.IsDefault = true;
            }
            else if (address.IsDefault)
            {
                await _repository.SetDefaultAddressAsync(userId, 0);
            }
            
            return await _repository.AddAddressAsync(address);
        }
        
        public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync(int userId)
        {
            var addresses = await _repository.GetAddressesByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<AddressDto>>(addresses);
        }
        
        public async Task<bool> UpdateAddressAsync(int addressId, CreateAddressDto addressDto)
        {
            var address = await _repository.GetAddressByIdAsync(addressId);
            if (address == null) return false;
            
            address.HouseNumber = addressDto.HouseNumber;
            address.StreetName = addressDto.StreetName;
            address.ColonyName = addressDto.ColonyName;
            address.City = addressDto.City;
            address.State = addressDto.State;
            address.Pincode = addressDto.Pincode;
            address.Landmark = addressDto.Landmark;
            
            await _repository.UpdateAddressAsync(address);
            return true;
        }
        
        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            return await _repository.DeleteAddressAsync(addressId);
        }
        
        public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
        {
            await _repository.SetDefaultAddressAsync(userId, addressId);
            return true;
        }
        
        // Admin methods
        public async Task<IEnumerable<UserProfile>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            var allUsers = await _repository.GetAllAsync();
            return allUsers.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
        
        public async Task<UserProfile?> SuspendUserAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return null;
            
            user.IsActive = false;
            await _repository.UpdateAsync(user);
            return user;
        }
        
        public async Task<UserProfile?> ReactivateUserAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return null;
            
            user.IsActive = true;
            await _repository.UpdateAsync(user);
            return user;
        }
        
        public async Task<int> GetTotalUserCountAsync()
        {
            var allUsers = await _repository.GetAllAsync();
            return allUsers.Count();
        }
    }
}