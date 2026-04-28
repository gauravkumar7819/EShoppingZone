using EShoppingZone.Order.API.DTOs;
using EShoppingZone.Order.API.Entities;

namespace EShoppingZone.Order.API.Services
{
    public interface IOrderService
    {
        Task<PlaceOrderResponseDto> PlaceOrderAsync(PlaceOrderDto placeOrderDto);
        Task<OrderDto?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId);
        Task<IEnumerable<OrderDto>> GetOrdersByMerchantIdAsync(int merchantId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> ChangeOrderStatusAsync(int orderId, UpdateOrderStatusDto updateDto);
        Task<bool> CancelOrderAsync(int orderId, string? reason);
        Task<bool> DeleteOrderAsync(int orderId);
        Task<decimal> ProcessOnlinePaymentAsync(int userId, decimal amount, int orderId);
        Task<OrderSummaryDto> GetOrderSummaryAsync(int orderId);
        
        // New tracking methods
        Task<IEnumerable<StatusHistoryDto>> GetOrderStatusHistoryAsync(int orderId);
        Task<OrderTrackingDto> GetOrderTrackingAsync(int orderId);
    }
    
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public DateTime OrderDate { get; set; }
        public string? DeliveryAddress { get; set; }
    }
    
    public class OrderTrackingDto
    {
        public int OrderId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? TrackingNumber { get; set; }
        public string? CancellationReason { get; set; }
        public List<StatusHistoryDto> StatusHistory { get; set; } = new();
    }
}