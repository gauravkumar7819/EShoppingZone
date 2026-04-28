using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Order.API.DTOs;
using EShoppingZone.Order.API.Services;

namespace EShoppingZone.Order.API.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<AdminOrderController> _logger;
        
        public AdminOrderController(IOrderService orderService, ILogger<AdminOrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        
        [HttpPut("{orderId}/status")]
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
        
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var result = await _orderService.DeleteOrderAsync(orderId);
            if (!result)
                return NotFound(new { error = "Order not found" });
                
            return Ok(new { message = "Order deleted successfully" });
        }
    }
}