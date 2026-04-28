using EShoppingZone.Profile.API.DTOs;
using EShoppingZone.Profile.API.Entities;

namespace EShoppingZone.Profile.API.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<LoginResponseDto> GoogleLoginAsync(string googleId, string email, string name);
        string GenerateJwtToken(UserProfile user);
    }
}