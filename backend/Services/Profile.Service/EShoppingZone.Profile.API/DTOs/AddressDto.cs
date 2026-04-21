using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Profile.API.DTOs
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string HouseNumber { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string ColonyName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string Landmark { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
    
    public class CreateAddressDto
    {
        [Required]
        public string HouseNumber { get; set; } = string.Empty;
        
        [Required]
        public string StreetName { get; set; } = string.Empty;
        
        [Required]
        public string ColonyName { get; set; } = string.Empty;
        
        [Required]
        public string City { get; set; } = string.Empty;
        
        [Required]
        public string State { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;
        
        public string Landmark { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
    }
}