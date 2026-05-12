using Moq;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using EShoppingZone.Cart.API.Services;
using EShoppingZone.Cart.API.Repositories;
using EShoppingZone.Cart.API.Entities;
using EShoppingZone.Cart.API.DTOs;
using CartModel = EShoppingZone.Cart.API.Entities.Cart;
using Xunit;

namespace EShoppingZone.Cart.Tests
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<CartService>> _loggerMock;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _repositoryMock = new Mock<ICartRepository>();
            _mapperMock = new Mock<IMapper>();
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<CartService>>();
            _cartService = new CartService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void CalculateCartTotal_ShouldReturnSumOfItems()
        {
            // Arrange
            var cart = new CartModel
            {
                Items = new List<CartItem>
                {
                    new CartItem { Price = 100, Quantity = 2 },
                    new CartItem { Price = 50, Quantity = 1 }
                }
            };

            // Act
            var total = _cartService.CalculateCartTotal(cart);

            // Assert
            Assert.Equal(250, total);
        }

        [Fact]
        public async Task GetCartByIdAsync_WhenCartDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            int cartId = 999;
            _repositoryMock.Setup(repo => repo.GetCartByIdAsync(cartId)).ReturnsAsync((CartModel?)null);

            // Act
            var result = await _cartService.GetCartByIdAsync(cartId);

            // Assert
            Assert.Null(result);
        }
    }
}
