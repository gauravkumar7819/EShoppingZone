using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShoppingZone.Order.API.Entities
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal => Price * Quantity;
        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        
        // Foreign Key
        public int OrderId { get; set; }
        
        // Navigation property
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
    }
}