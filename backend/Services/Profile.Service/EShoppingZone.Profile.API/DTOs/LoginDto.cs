using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Profile.API.DTOs
{
    public class LoginDto
    {
        [Required]
        public string EmailOrMobile { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}