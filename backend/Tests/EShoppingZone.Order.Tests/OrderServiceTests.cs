using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using EShoppingZone.Order.API.Services;
using EShoppingZone.Order.API.Repositories;
using EShoppingZone.Order.API.Entities;
using EShoppingZone.Order.API.Integrations;
using EShoppingZone.Order.API.DTOs;
using OrderModel = EShoppingZone.Order.API.Entities.Order;
using Xunit;

namespace EShoppingZone.Order.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICartServiceClient> _cartServiceMock;
        private readonly Mock<IWalletServiceClient> _walletServiceMock;
        private readonly Mock<IProfileServiceClient> _profileServiceMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _repositoryMock = new Mock<IOrderRepository>();
            _mapperMock = new Mock<IMapper>();
            _cartServiceMock = new Mock<ICartServiceClient>();
            _walletServiceMock = new Mock<IWalletServiceClient>();
            _profileServiceMock = new Mock<IProfileServiceClient>();
            _loggerMock = new Mock<ILogger<OrderService>>();

            _orderService = new OrderService(
                _repositoryMock.Object,
                _mapperMock.Object,
                _cartServiceMock.Object,
                _walletServiceMock.Object,
                _profileServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenOrderExists_ShouldReturnOrderDto()
        {
            // Arrange
            int orderId = 1;
            var order = new OrderModel { OrderId = orderId, TotalAmount = 500 };
            var orderDto = new OrderDto { OrderId = orderId, TotalAmount = 500 };

            _repositoryMock.Setup(repo => repo.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            _mapperMock.Setup(m => m.Map<OrderDto>(order)).Returns(orderDto);

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.OrderId);
        }
    }
}
