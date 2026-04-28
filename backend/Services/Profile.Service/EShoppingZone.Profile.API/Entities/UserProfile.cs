using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShoppingZone.Profile.API.Entities
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(15)]
        public string MobileNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Customer";
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string About { get; set; } = string.Empty;
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public IList<Address> Addresses { get; set; } = new List<Address>();
        
        public string? GoogleId { get; set; }
        
        public bool IsEmailVerified { get; set; } = false;
    }
}