using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Profile.API.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        [MaxLength(15)]
        public string MobileNumber { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = "Customer";
        
        public string? About { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        public string? Gender { get; set; }
    }
}