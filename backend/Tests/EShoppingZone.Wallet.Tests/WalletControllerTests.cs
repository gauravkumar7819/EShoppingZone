using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EShoppingZone.Wallet.API.Controllers;
using EShoppingZone.Wallet.API.Services;
using EShoppingZone.Wallet.API.DTOs;
using Xunit;

namespace EShoppingZone.Wallet.Tests
{
    public class WalletControllerTests
    {
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly Mock<IRazorpayService> _razorpayServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<WalletController>> _loggerMock;
        private readonly WalletController _controller;

        public WalletControllerTests()
        {
            _walletServiceMock = new Mock<IWalletService>();
            _razorpayServiceMock = new Mock<IRazorpayService>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<WalletController>>();

            _controller = new WalletController(
                _walletServiceMock.Object,
                _razorpayServiceMock.Object,
                _configurationMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetWalletById_WhenWalletExists_ShouldReturnOk()
        {
            // Arrange
            int walletId = 1;
            var walletDto = new WalletDto { Id = walletId, CustomerId = 101, CurrentBalance = 500 };
            _walletServiceMock.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(walletDto);

            // Act
            var result = await _controller.GetWalletById(walletId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedWallet = Assert.IsType<WalletDto>(okResult.Value);
            Assert.Equal(walletId, returnedWallet.Id);
        }

        [Fact]
        public async Task GetWalletById_WhenWalletDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            int walletId = 99;
            _walletServiceMock.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync((WalletDto)null);

            // Act
            var result = await _controller.GetWalletById(walletId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AddMoney_WithValidAmount_ShouldReturnOk()
        {
            // Arrange
            var addMoneyDto = new AddMoneyDto { WalletId = 1, Amount = 100, Remarks = "Test deposit" };
            var expectedResponse = new TransactionResponseDto { Success = true, Message = "Success" };
            
            _walletServiceMock.Setup(s => s.AddMoneyAsync(1, 100, "Test deposit", null))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AddMoney(addMoneyDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task AddMoney_WithInvalidAmount_ShouldReturnBadRequest()
        {
            // Arrange
            var addMoneyDto = new AddMoneyDto { WalletId = 1, Amount = -50 };

            // Act
            var result = await _controller.AddMoney(addMoneyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
