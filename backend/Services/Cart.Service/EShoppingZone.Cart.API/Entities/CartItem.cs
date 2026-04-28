using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShoppingZone.Cart.API.Entities
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public int MerchantId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        
        // Foreign Key
        public int CartId { get; set; }
        
        // Navigation property
        [ForeignKey("CartId")]
        public Cart Cart { get; set; } = null!;
        
        [NotMapped]
        public decimal Subtotal => Price * Quantity;
    }
}