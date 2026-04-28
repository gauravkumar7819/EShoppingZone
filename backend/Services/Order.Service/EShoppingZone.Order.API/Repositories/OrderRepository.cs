using Microsoft.EntityFrameworkCore;
using EShoppingZone.Order.API.Data;
using EShoppingZone.Order.API.Entities;
using OrderModel= EShoppingZone.Order.API.Entities.Order;
namespace EShoppingZone.Order.API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;
        
        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }
        
        public async Task<OrderModel?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistories)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
        
        public async Task<IEnumerable<OrderModel>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistories)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<OrderModel>> GetOrdersByMerchantIdAsync(int merchantId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistories)
                .Where(o => o.MerchantId == merchantId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistories)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<OrderModel>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<OrderModel> CreateOrderAsync(OrderModel order)
        {
            order.CreatedAt = DateTime.UtcNow;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }
        
        public async Task<OrderModel> UpdateOrderAsync(OrderModel order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return order;
        }
        
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null) return false;
            
            order.Status = OrderStatus.Cancelled;
            order.CancelledDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> OrderExistsAsync(int orderId)
        {
            return await _context.Orders.AnyAsync(o => o.OrderId == orderId);
        }
        
        public async Task<OrderItem> AddOrderItemAsync(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
            return orderItem;
        }
        
        public async Task AddStatusHistoryAsync(StatusHistory statusHistory)
        {
            _context.StatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();
        }
        
        public async Task<IEnumerable<StatusHistory>> GetStatusHistoryAsync(int orderId)
        {
            return await _context.StatusHistories
                .Where(sh => sh.OrderId == orderId)
                .OrderByDescending(sh => sh.ChangedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderModel>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<int> GetOrderCountByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .CountAsync(o => o.CustomerId == customerId && o.Status != OrderStatus.Cancelled);
        }
        
        public async Task<decimal> GetTotalSpentByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);
        }
    }
}