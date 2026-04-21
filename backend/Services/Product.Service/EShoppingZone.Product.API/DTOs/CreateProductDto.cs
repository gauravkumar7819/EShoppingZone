using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Product.API.DTOs
{
    public class CreateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal MRP { get; set; }
        
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        
        [MaxLength(500)]
        public string Brand { get; set; } = string.Empty;
        
        public List<string> Images { get; set; } = new(); // Google Images URLs
        
        public Dictionary<string, string> Specifications { get; set; } = new();
        
        public int MerchantId { get; set; }
    }
    
    public class UpdateProductDto
    {
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal MRP { get; set; }
        
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        
        public List<string> Images { get; set; } = new(); // Google Images URLs
        
        public Dictionary<string, string> Specifications { get; set; } = new();
        
        public bool IsActive { get; set; }
    }
    
    public class AddReviewDto
    {
        [Required]
        [Range(1, 5)]
        public double Rating { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Review { get; set; } = string.Empty;
        
        public int UserId { get; set; }
    }
}