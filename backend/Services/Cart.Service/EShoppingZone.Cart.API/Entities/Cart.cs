using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShoppingZone.Cart.API.Entities
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; } // Equals UserId after login
        
        public int UserId { get; set; } // Reference to UserProfile
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public IList<CartItem> Items { get; set; } = new List<CartItem>();
        
        // Computed property (not stored in database)
        [NotMapped]
        public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);
        
        [NotMapped]
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}