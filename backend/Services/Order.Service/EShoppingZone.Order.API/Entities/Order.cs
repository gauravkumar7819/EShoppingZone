using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShoppingZone.Order.API.Entities
{
    public enum OrderStatus
    {
        Placed = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Refunded = 7
    }
    
    public enum PaymentMode
    {
        COD = 1,
        Wallet = 2,
        Card = 3,
        UPI = 4
    }
    
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        public int? MerchantId { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Placed;
        
        [Required]
        public PaymentMode ModeOfPayment { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public int DeliveryAddressId { get; set; }
        
        [MaxLength(500)]
        public string? DeliveryAddress { get; set; } // Cached address at time of order
        
        [MaxLength(100)]
        public string? TrackingNumber { get; set; }
        
        [MaxLength(500)]
        public string? CancellationReason { get; set; }
        
        public DateTime? PaymentDate { get; set; }
        
        public string? TransactionId { get; set; }
        
        public DateTime? ShippedDate { get; set; }
        
        public DateTime? EstimatedDeliveryDate { get; set; }
        
        public DateTime? DeliveredDate { get; set; }
        
        public DateTime? CancelledDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public IList<OrderItem> Items { get; set; } = new List<OrderItem>();
        
        public IList<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
    }
}