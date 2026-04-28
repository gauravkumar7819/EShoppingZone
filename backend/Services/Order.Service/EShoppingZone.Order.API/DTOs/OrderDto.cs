using EShoppingZone.Order.API.Entities;

namespace EShoppingZone.Order.API.DTOs
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ModeOfPayment { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int DeliveryAddressId { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? CancellationReason { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<StatusHistoryDto> StatusHistories { get; set; } = new();
    }
    
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string? ImageUrl { get; set; }
    }
    
    public class StatusHistoryDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? Remarks { get; set; }
        public string? ChangedBy { get; set; }
    }
    
    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string? TrackingNumber { get; set; }
    }
    
    public class CancelOrderDto
    {
        public string? Reason { get; set; }
    }
}