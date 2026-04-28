using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace EShoppingZone.Product.API.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty; // Electronics, Books, Apparel, Personal Care
        
        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty; // Mobile, Laptop, Shirt, etc.
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MRP { get; set; }
        
        public int StockQuantity { get; set; }
        
        [MaxLength(500)]
        public string Brand { get; set; } = string.Empty;
        
        // Complex types stored as JSON
        [Column(TypeName = "nvarchar(max)")]
        public string RatingJson { get; set; } = "{}";
        
        [Column(TypeName = "nvarchar(max)")]
        public string ReviewJson { get; set; } = "{}";
        
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(max)")]
        public string SpecificationsJson { get; set; } = "{}";
        
        // Navigation properties (not stored)
        [NotMapped]
        public Dictionary<int, double> Ratings
        {
            get => JsonConvert.DeserializeObject<Dictionary<int, double>>(string.IsNullOrEmpty(RatingJson) ? "{}" : RatingJson) ?? new Dictionary<int, double>();
            set => RatingJson = JsonConvert.SerializeObject(value ?? new Dictionary<int, double>());
        }
        
        [NotMapped]
        public Dictionary<int, string> Reviews
        {
            get => JsonConvert.DeserializeObject<Dictionary<int, string>>(string.IsNullOrEmpty(ReviewJson) ? "{}" : ReviewJson) ?? new Dictionary<int, string>();
            set => ReviewJson = JsonConvert.SerializeObject(value ?? new Dictionary<int, string>());
        }
        
        [NotMapped]
        public Dictionary<string, string> Specifications
        {
            get => JsonConvert.DeserializeObject<Dictionary<string, string>>(string.IsNullOrEmpty(SpecificationsJson) ? "{}" : SpecificationsJson) ?? new Dictionary<string, string>();
            set => SpecificationsJson = JsonConvert.SerializeObject(value ?? new Dictionary<string, string>());
        }
        
        public int MerchantId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Computed property for average rating
        [NotMapped]
        public double AverageRating => Ratings.Count > 0 ? Ratings.Values.Average() : 0;
        
        [NotMapped]
        public int TotalReviews => Reviews.Count;
    }
}