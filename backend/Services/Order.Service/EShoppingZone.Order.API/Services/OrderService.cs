using AutoMapper;
using EShoppingZone.Order.API.DTOs;
using EShoppingZone.Order.API.Entities;
using EShoppingZone.Order.API.Repositories;
using EShoppingZone.Order.API.Integrations;
using OrderModel=EShoppingZone.Order.API.Entities.Order;
namespace EShoppingZone.Order.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICartServiceClient _cartService;
        private readonly IWalletServiceClient _walletService;
        private readonly IProfileServiceClient _profileService;
        private readonly ILogger<OrderService> _logger;
        
        public OrderService(
            IOrderRepository repository,
            IMapper mapper,
            ICartServiceClient cartService,
            IWalletServiceClient walletService,
            IProfileServiceClient profileService,
            ILogger<OrderService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _cartService = cartService;
            _walletService = walletService;
            _profileService = profileService;
            _logger = logger;
        }
        
        public async Task<PlaceOrderResponseDto> PlaceOrderAsync(PlaceOrderDto placeOrderDto)
        {
            // Validate payment mode
            var isValidPaymentMode = Enum.TryParse<PaymentMode>(placeOrderDto.ModeOfPayment, true, out var paymentMode);
            if (!isValidPaymentMode)
                throw new ArgumentException("Invalid payment mode. Use COD or Wallet");
            
            // Get cart from Cart Service
            var cart = await _cartService.GetCartByIdAsync(placeOrderDto.CartId);
            if (cart == null || cart.Items == null || !cart.Items.Any())
                throw new InvalidOperationException("Cart is empty or not found");
            
            // Get delivery address
            var address = await _profileService.GetFormattedAddressAsync(placeOrderDto.DeliveryAddressId);
            if (string.IsNullOrEmpty(address))
                throw new InvalidOperationException("Delivery address not found");
            
            // Calculate total amount and get merchant ID from first product
            var totalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
            var merchantId = cart.Items.FirstOrDefault()?.MerchantId;
            
            try
            {
                // Generate transactionId for COD; Wallet transactionId is assigned after payment below.
                string? transactionId = paymentMode == PaymentMode.COD
                    ? $"COD_{DateTime.UtcNow.Ticks}"
                    : null;

                // For COD: order creation proceeds immediately.
                // For Wallet: payment is processed AFTER order creation (real OrderId needed).
                // Create order
                var order = new OrderModel
                {
                    CustomerId = placeOrderDto.CustomerId,
                    MerchantId = merchantId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Placed,
                    ModeOfPayment = paymentMode,
                    TotalAmount = totalAmount,
                    DeliveryAddressId = placeOrderDto.DeliveryAddressId,
                    DeliveryAddress = address,
                    TransactionId = transactionId,
                    PaymentDate = DateTime.UtcNow
                };

                order = await _repository.CreateOrderAsync(order);
                
                // For Wallet payment: process payment now that we have the real OrderId
                if (paymentMode == PaymentMode.Wallet)
                {
                    var walletResult = await _walletService.GetWalletByCustomerIdAsync(placeOrderDto.CustomerId);
                    if (walletResult == null)
                    {
                        await _repository.DeleteOrderAsync(order.OrderId);
                        return new PlaceOrderResponseDto
                        {
                            Success = false,
                            PaymentSuccess = false,
                            Message = "Wallet not found. Payment failed.",
                            TotalAmount = totalAmount
                        };
                    }
                    
                    var paymentResult = await _walletService.ProcessPaymentAsync(
                        walletResult.WalletId,
                        totalAmount,
                        order.OrderId,
                        $"Payment for order {order.OrderId}"
                    );
                    
                    if (!paymentResult.Success)
                    {
                        await _repository.DeleteOrderAsync(order.OrderId);
                        return new PlaceOrderResponseDto
                        {
                            Success = false,
                            PaymentSuccess = false,
                            Message = paymentResult.Message ?? "Wallet payment failed. Insufficient balance?",
                            TotalAmount = totalAmount
                        };
                    }
                    
                    transactionId = paymentResult.TransactionId;
                    
                    // Update order with transaction info
                    order.TransactionId = transactionId;
                    order.PaymentDate = DateTime.UtcNow;
                    await _repository.UpdateOrderAsync(order);
                }
                
                // Create order items
                foreach (var cartItem in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.ProductName,
                        Price = cartItem.Price,
                        Quantity = cartItem.Quantity,
                        ImageUrl = cartItem.ImageUrl
                    };
                    await _repository.AddOrderItemAsync(orderItem);
                }
                
                // Add status history
                await _repository.AddStatusHistoryAsync(new StatusHistory
                {
                    OrderId = order.OrderId,
                    Status = OrderStatus.Placed,
                    Remarks = "Order placed successfully",
                    ChangedBy = placeOrderDto.CustomerId.ToString()
                });
                
                // Clear cart
                await _cartService.ClearCartAsync(placeOrderDto.CartId);
                
                _logger.LogInformation("Order {OrderId} placed successfully for customer {CustomerId}", 
                    order.OrderId, placeOrderDto.CustomerId);
                
                return new PlaceOrderResponseDto
                {
                    Success = true,
                    OrderId = order.OrderId,
                    TotalAmount = totalAmount,
                    Status = order.Status.ToString(),
                    Message = "Order placed successfully",
                    PaymentSuccess = true,
                    TransactionId = transactionId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order for customer {CustomerId}", placeOrderDto.CustomerId);
                throw;
            }
        }
        
        public async Task<decimal> ProcessOnlinePaymentAsync(int userId, decimal amount, int orderId)
        {
            try
            {
                // Get wallet by customer ID
                var wallet = await _walletService.GetWalletByCustomerIdAsync(userId);
                if (wallet == null)
                {
                    _logger.LogError("Wallet not found for customer {CustomerId}", userId);
                    return 0;
                }

                // Process payment using wallet ID
                var result = await _walletService.ProcessPaymentAsync(
                    wallet.WalletId,
                    amount,
                    orderId,
                    $"Payment for order {orderId}"
                );
                
                if (result.Success && decimal.TryParse(result.TransactionId, out var transactionId))
                {
                    return transactionId;
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for user {UserId}", userId);
                return 0;
            }
        }
        
        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _repository.GetOrderByIdAsync(orderId);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }
        
        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _repository.GetOrdersByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }
        
        public async Task<IEnumerable<OrderDto>> GetOrdersByMerchantIdAsync(int merchantId)
        {
            var orders = await _repository.GetOrdersByMerchantIdAsync(merchantId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }
        
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _repository.GetAllOrdersAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }
        
        public async Task<OrderDto?> ChangeOrderStatusAsync(int orderId, UpdateOrderStatusDto updateDto)
        {
            var order = await _repository.GetOrderByIdAsync(orderId);
            if (order == null) return null;
            
            var newStatus = Enum.Parse<OrderStatus>(updateDto.Status, true);
            
            order.Status = newStatus;
            order.TrackingNumber = updateDto.TrackingNumber ?? order.TrackingNumber;
            order.UpdatedAt = DateTime.UtcNow;
            
            // Update specific dates based on status
            switch (newStatus)
            {
                case OrderStatus.Shipped:
                    order.ShippedDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered:
                    order.DeliveredDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Cancelled:
                    order.CancelledDate = DateTime.UtcNow;
                    break;
            }
            
            await _repository.UpdateOrderAsync(order);
            
            // Add status history
            await _repository.AddStatusHistoryAsync(new StatusHistory
            {
                OrderId = orderId,
                Status = newStatus,
                Remarks = updateDto.Remarks,
                ChangedBy = "Admin"
            });
            
            return _mapper.Map<OrderDto>(order);
        }
        
        public async Task<bool> CancelOrderAsync(int orderId, string? reason)
        {
            var order = await _repository.GetOrderByIdAsync(orderId);
            if (order == null) return false;
            
            // Only allow cancellation for orders that are not delivered
            if (order.Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Delivered orders cannot be cancelled");
            
            order.Status = OrderStatus.Cancelled;
            order.CancelledDate = DateTime.UtcNow;
            order.CancellationReason = reason;
            order.UpdatedAt = DateTime.UtcNow;
            
            await _repository.UpdateOrderAsync(order);
            
            // Add status history
            await _repository.AddStatusHistoryAsync(new StatusHistory
            {
                OrderId = orderId,
                Status = OrderStatus.Cancelled,
                Remarks = reason ?? "Order cancelled by customer",
                ChangedBy = order.CustomerId.ToString()
            });
            
            // Process refund if payment was made via wallet
            if (order.ModeOfPayment == PaymentMode.Wallet && !string.IsNullOrEmpty(order.TransactionId))
            {
                // Call wallet service for refund
                _logger.LogInformation("Processing refund for order {OrderId}", orderId);
            }
            
            return true;
        }
        
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            return await _repository.DeleteOrderAsync(orderId);
        }
        
        public async Task<OrderSummaryDto> GetOrderSummaryAsync(int orderId)
        {
            var order = await _repository.GetOrderByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");
            
            return new OrderSummaryDto
            {
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ItemCount = order.Items.Sum(i => i.Quantity),
                OrderDate = order.OrderDate,
                DeliveryAddress = order.DeliveryAddress
            };
        }

        public async Task<IEnumerable<StatusHistoryDto>> GetOrderStatusHistoryAsync(int orderId)
        {
            var order = await _repository.GetOrderByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");

            var statusHistory = await _repository.GetStatusHistoryAsync(orderId);
            return _mapper.Map<IEnumerable<StatusHistoryDto>>(statusHistory);
        }

        public async Task<OrderTrackingDto> GetOrderTrackingAsync(int orderId)
        {
            var order = await _repository.GetOrderByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");

            var statusHistory = await _repository.GetStatusHistoryAsync(orderId);

            return new OrderTrackingDto
            {
                OrderId = order.OrderId,
                CurrentStatus = order.Status.ToString(),
                OrderDate = order.OrderDate,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                TrackingNumber = order.TrackingNumber,
                CancellationReason = order.CancellationReason,
                StatusHistory = _mapper.Map<List<StatusHistoryDto>>(statusHistory)
            };
        }
    }
}