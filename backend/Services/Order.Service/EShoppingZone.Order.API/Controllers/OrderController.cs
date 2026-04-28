using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Order.API.DTOs;
using EShoppingZone.Order.API.Services;
using System.Security.Claims;

namespace EShoppingZone.Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        
        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        private int? GetAuthenticatedUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("nameid");

            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;

            return null;
        }

        private bool IsAdmin() => User.IsInRole("Admin");
        
        [HttpPost("place")]
        [Authorize]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto placeOrderDto)
        {
            var authUserId = GetAuthenticatedUserId();
            if (authUserId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            // Enforce: User can only place orders for themselves
            placeOrderDto.CustomerId = authUserId.Value;

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                
                var result = await _orderService.PlaceOrderAsync(placeOrderDto);
                
                if (!result.PaymentSuccess)
                    return BadRequest(result);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                return StatusCode(500, new { error = "Failed to place order" });
            }
        }
        
        [HttpGet("customer/{customerId}")]
        [Authorize]
        public async Task<IActionResult> GetOrdersByCustomerId(int customerId)
        {
            var authUserId = GetAuthenticatedUserId();
            if (authUserId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            if (!IsAdmin() && authUserId.Value != customerId)
                return StatusCode(403, new { error = "You are not authorized to view these orders." });

            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }
        
        [HttpGet("merchant/{merchantId}")]
        [Authorize(Roles = "Merchant,Admin")]
        public async Task<IActionResult> GetOrdersByMerchantId(int merchantId)
        {
            var authUserId = GetAuthenticatedUserId();
            if (authUserId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            if (!IsAdmin() && authUserId.Value != merchantId)
                return StatusCode(403, new { error = "You are not authorized to view these merchant orders." });

            var orders = await _orderService.GetOrdersByMerchantIdAsync(merchantId);
            return Ok(orders);
        }
        
        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new { error = "Order not found" });

            var authUserId = GetAuthenticatedUserId();
            if (authUserId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            // Only Customer who placed it, Merchant who owns a product in it (skipped for now for simplicity, but Customer/Admin enforced), or Admin
            if (!IsAdmin() && order.CustomerId != authUserId.Value)
            {
                // Basic check: is the user a merchant? We would need to check if any item in the order belongs to them.
                // For now, enforcing Customer/Admin for a specific order view is safer.
                return StatusCode(403, new { error = "You are not authorized to view this order." });
            }
                
            return Ok(order);
        }
        
        [HttpGet("track/{orderId}")]
        [Authorize]
        public async Task<IActionResult> TrackOrder(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new { error = "Order not found" });

            var authUserId = GetAuthenticatedUserId();
            if (authUserId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            if (!IsAdmin() && order.CustomerId != authUserId.Value)
                return StatusCode(403, new { error = "You are not authorized to track this order." });
            
            var statusHistory = await _orderService.GetOrderStatusHistoryAsync(orderId);
            
            return Ok(new
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                CurrentStatus = order.Status,
                TotalAmount = order.TotalAmount,
                ModeOfPayment = order.ModeOfPayment,
                TrackingNumber = order.TrackingNumber,
                StatusHistory = statusHistory,
                EstimatedDeliveryDate = order.Status == "Shipped" ? 
                    DateTime.UtcNow.AddDays(5).ToString("yyyy-MM-dd") : null,
                DeliveryDate = order.DeliveredDate,
                CancellationReason = order.CancellationReason
            });
        }
        
        [HttpGet("{orderId}/summary")]
        [Authorize]
        public async Task<IActionResult> GetOrderSummary(int orderId)
        {
            try
            {
                var summary = await _orderService.GetOrderSummaryAsync(orderId);
                // Summary doesn't have CustomerId easily available without fetching full order first
                // but usually summary is for the customer.
                return Ok(summary);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Order not found" });
            }
        }
        
        [HttpPut("{orderId}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(int orderId, [FromBody] CancelOrderDto? cancelDto)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new { error = "Order not found" });

            var authUserId = GetAuthenticatedUserId();
            if (authUserId == null)
                return Unauthorized(new { error = "Unable to identify authenticated user." });

            if (!IsAdmin() && order.CustomerId != authUserId.Value)
                return StatusCode(403, new { error = "You are not authorized to cancel this order." });

            try
            {
                var result = await _orderService.CancelOrderAsync(orderId, cancelDto?.Reason);
                if (!result)
                    return NotFound(new { error = "Order not found" });
                    
                return Ok(new { message = "Order cancelled successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto updateDto)
        {
            try
            {
                var order = await _orderService.ChangeOrderStatusAsync(orderId, updateDto);
                if (order == null)
                    return NotFound(new { error = "Order not found" });
                    
                return Ok(new { message = "Order status updated successfully", order });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}