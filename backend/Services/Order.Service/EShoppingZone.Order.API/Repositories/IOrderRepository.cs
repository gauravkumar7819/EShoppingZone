using EShoppingZone.Order.API.Entities;
using OrderModel= EShoppingZone.Order.API.Entities.Order;
namespace EShoppingZone.Order.API.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderModel?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderModel>> GetOrdersByCustomerIdAsync(int customerId);
        Task<IEnumerable<OrderModel>> GetOrdersByMerchantIdAsync(int merchantId);
        Task<IEnumerable<OrderModel>> GetAllOrdersAsync();
        Task<IEnumerable<OrderModel>> GetOrdersByStatusAsync(OrderStatus status);
        Task<OrderModel> CreateOrderAsync(OrderModel order);
        Task<OrderModel> UpdateOrderAsync(OrderModel order);
        Task<bool> DeleteOrderAsync(int orderId);
        Task<bool> OrderExistsAsync(int orderId);
        Task<OrderItem> AddOrderItemAsync(OrderItem orderItem);
        Task AddStatusHistoryAsync(StatusHistory statusHistory);
        Task<IEnumerable<StatusHistory>> GetStatusHistoryAsync(int orderId);
        Task<IEnumerable<OrderModel>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOrderCountByCustomerAsync(int customerId);
        Task<decimal> GetTotalSpentByCustomerAsync(int customerId);
    }
}