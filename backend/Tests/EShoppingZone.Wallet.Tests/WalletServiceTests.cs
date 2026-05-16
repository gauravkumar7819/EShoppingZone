using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using EShoppingZone.Wallet.API.Services;
using EShoppingZone.Wallet.API.Repositories;
using EShoppingZone.Wallet.API.Entities;
using EShoppingZone.Wallet.API.Data;
using EShoppingZone.Wallet.API.DTOs;
using Xunit;

namespace EShoppingZone.Wallet.Tests
{
    public class WalletServiceTests
    {
        private readonly Mock<IWalletRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<WalletService>> _loggerMock;
        private readonly WalletDbContext _context;
        private readonly WalletService _walletService;

        public WalletServiceTests()
        {
            _repositoryMock = new Mock<IWalletRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<WalletService>>();

            var options = new DbContextOptionsBuilder<WalletDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new WalletDbContext(options);

            _walletService = new WalletService(
                _repositoryMock.Object,
                _context,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetWalletByCustomerIdAsync_WhenWalletExists_ShouldReturnWalletDto()
        {
            // Arrange
            int customerId = 1;
            var wallet = new EWallet { CustomerId = customerId, CurrentBalance = 1000 };
            var walletDto = new WalletDto { CustomerId = customerId, CurrentBalance = 1000 };

            _repositoryMock.Setup(repo => repo.GetWalletByCustomerIdAsync(customerId)).ReturnsAsync(wallet);
            _mapperMock.Setup(m => m.Map<WalletDto>(wallet)).Returns(walletDto);

            // Act
            var result = await _walletService.GetWalletByCustomerIdAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.CurrentBalance);
        }
    }
}
