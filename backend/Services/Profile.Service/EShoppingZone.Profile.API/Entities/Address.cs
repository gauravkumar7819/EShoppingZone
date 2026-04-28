using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShoppingZone.Profile.API.Entities
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string HouseNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string StreetName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string ColonyName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string State { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Landmark { get; set; } = string.Empty;
        
        public bool IsDefault { get; set; } = false;
        
        public int UserProfileId { get; set; }
        
        [ForeignKey("UserProfileId")]
        public UserProfile UserProfile { get; set; } = null!;
    }
}