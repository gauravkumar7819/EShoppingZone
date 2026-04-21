using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using EShoppingZone.Profile.API.DTOs;
using EShoppingZone.Profile.API.Entities;
using EShoppingZone.Profile.API.Repositories;

namespace EShoppingZone.Profile.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IProfileRepository _repository;
        private readonly IPasswordHasher<UserProfile> _passwordHasher;
        private readonly IConfiguration _configuration;
        
        public AuthService(
            IProfileRepository repository,
            IPasswordHasher<UserProfile> passwordHasher,
            IConfiguration configuration)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }
        
        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            UserProfile? user = null;
            
            if (loginDto.EmailOrMobile.Contains("@"))
            {
                user = await _repository.GetByEmailAsync(loginDto.EmailOrMobile);
            }
            else
            {
                user = await _repository.GetByMobileNumberAsync(loginDto.EmailOrMobile);
            }
            
            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");
                
            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated");
            
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid credentials");
            
            return new LoginResponseDto
            {
                Token = GenerateJwtToken(user),
                Role = user.Role,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }
        
        public async Task<LoginResponseDto> GitHubLoginAsync(string gitHubId, string email, string name)
        {
            var user = await _repository.GetByGitHubIdAsync(gitHubId);
            
            if (user == null)
            {
                var existingUser = await _repository.GetByEmailAsync(email);
                if (existingUser != null)
                {
                    existingUser.GitHubId = gitHubId;
                    user = await _repository.UpdateAsync(existingUser);
                }
                else
                {
                    user = new UserProfile
                    {
                        FullName = name,
                        Email = email,
                        Role = "Customer",
                        GitHubId = gitHubId,
                        IsActive = true,
                        IsEmailVerified = true
                    };
                    user.PasswordHash = _passwordHasher.HashPassword(user, Guid.NewGuid().ToString());
                    user = await _repository.CreateAsync(user);
                }
            }
            
            return new LoginResponseDto
            {
                Token = GenerateJwtToken(user),
                Role = user.Role,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }
        
        public string GenerateJwtToken(UserProfile user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "your-super-secret-key-minimum-32-characters-long!");
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}