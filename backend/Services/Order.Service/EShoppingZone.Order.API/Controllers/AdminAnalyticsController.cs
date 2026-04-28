using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Order.API.Services;
using EShoppingZone.Order.API.Integrations;

namespace EShoppingZone.Order.API.Controllers
{
    [ApiController]
    [Route("api/admin/analytics")]
    [Authorize(Roles = "Admin")]
    public class AdminAnalyticsController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IProfileServiceClient _profileService;
        private readonly ILogger<AdminAnalyticsController> _logger;
        
        public AdminAnalyticsController(
            IOrderService orderService,
            IProfileServiceClient profileService,
            ILogger<AdminAnalyticsController> logger)
        {
            _orderService = orderService;
            _profileService = profileService;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetDashboardAnalytics()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                var users = await _profileService.GetAllUsersAsync();
                
                var totalOrders = orders.Count();
                var completedOrders = orders.Count(o => o.Status == "Delivered");
                var pendingOrders = orders.Count(o => o.Status == "Placed" || o.Status == "Confirmed" || o.Status == "Processing");
                var cancelledOrders = orders.Count(o => o.Status == "Cancelled");
                
                var totalRevenue = orders
                    .Where(o => o.Status == "Delivered")
                    .Sum(o => o.TotalAmount);
                
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
                
                // Top 10 products
                var topProducts = orders
                    .SelectMany(o => o.Items)
                    .GroupBy(i => new { i.ProductId, i.ProductName })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        TotalQuantity = g.Sum(i => i.Quantity),
                        TotalRevenue = g.Sum(i => i.Subtotal)
                    })
                    .OrderByDescending(p => p.TotalRevenue)
                    .Take(10)
                    .ToList();
                
                // Monthly revenue for last 12 months
                var monthlyRevenue = orders
                    .Where(o => o.Status == "Delivered")
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => m.Month)
                    .Take(12)
                    .ToList();
                
                // Order status distribution
                var statusDistribution = orders
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        Percentage = orders.Count() > 0 ? (g.Count() * 100.0 / orders.Count()) : 0
                    })
                    .ToList();
                
                return Ok(new
                {
                    Summary = new
                    {
                        TotalUsers = users?.Count() ?? 0,
                        TotalOrders = totalOrders,
                        CompletedOrders = completedOrders,
                        PendingOrders = pendingOrders,
                        CancelledOrders = cancelledOrders,
                        TotalRevenue = totalRevenue,
                        AverageOrderValue = averageOrderValue,
                        ConversionRate = users != null && users.Any() ? (totalOrders * 100.0 / users.Count()) : 0
                    },
                    TopProducts = topProducts,
                    MonthlyRevenue = monthlyRevenue,
                    StatusDistribution = statusDistribution,
                    LastUpdated = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard analytics");
                return StatusCode(500, new { error = "Failed to generate analytics" });
            }
        }
        
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueAnalytics(
            [FromQuery] int days = 30,
            [FromQuery] string? period = "daily")
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                var startDate = DateTime.UtcNow.AddDays(-days);
                
                var deliveredOrders = orders
                    .Where(o => o.Status == "Delivered" && o.OrderDate >= startDate)
                    .ToList();
                
                if (period?.ToLower() == "monthly")
                {
                    var monthlyData = deliveredOrders
                        .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                        .Select(g => new
                        {
                            Period = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                            Revenue = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(m => m.Period)
                        .ToList();
                    
                    return Ok(monthlyData);
                }
                else // daily
                {
                    var dailyData = deliveredOrders
                        .GroupBy(o => o.OrderDate.Date)
                        .Select(g => new
                        {
                            Date = g.Key.ToString("yyyy-MM-dd"),
                            Revenue = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(d => d.Date)
                        .ToList();
                    
                    return Ok(dailyData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating revenue analytics");
                return StatusCode(500, new { error = "Failed to generate revenue analytics" });
            }
        }
        
        [HttpGet("orders/status")]
        public async Task<IActionResult> GetOrderStatusAnalytics()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                
                var statusData = orders
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList();
                
                return Ok(statusData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating order status analytics");
                return StatusCode(500, new { error = "Failed to generate order status analytics" });
            }
        }
        
        [HttpGet("customers/top")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int limit = 10)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                
                var topCustomers = orders
                    .Where(o => o.Status == "Delivered")
                    .GroupBy(o => new { o.CustomerId, o.CustomerName })
                    .Select(g => new
                    {
                        CustomerId = g.Key.CustomerId,
                        CustomerName = g.Key.CustomerName,
                        TotalOrders = g.Count(),
                        TotalSpent = g.Sum(o => o.TotalAmount),
                        AverageOrderValue = g.Average(o => o.TotalAmount)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(limit)
                    .ToList();
                
                return Ok(topCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating top customers analytics");
                return StatusCode(500, new { error = "Failed to generate top customers analytics" });
            }
        }
    }
}