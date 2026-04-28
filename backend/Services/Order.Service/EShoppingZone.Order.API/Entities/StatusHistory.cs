using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShoppingZone.Order.API.Entities
{
    public class StatusHistory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public OrderStatus Status { get; set; }
        
        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string? Remarks { get; set; }
        
        [MaxLength(100)]
        public string? ChangedBy { get; set; }
        
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
    }
}