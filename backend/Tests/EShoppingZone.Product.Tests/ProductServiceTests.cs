using Moq;
using AutoMapper;
using EShoppingZone.Product.API.Services;
using EShoppingZone.Product.API.Repositories;
using EShoppingZone.Product.API.Entities;
using EShoppingZone.Product.API.DTOs;
using ProductModel = EShoppingZone.Product.API.Entities.Product;
using Xunit;

namespace EShoppingZone.Product.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _mapperMock = new Mock<IMapper>();
            _productService = new ProductService(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProductDetailDto()
        {
            // Arrange
            int productId = 1;
            var product = new ProductModel { Id = productId, Name = "Test Product" };
            var productDetailDto = new ProductDetailDto { Id = productId, Name = "Test Product" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            _mapperMock.Setup(m => m.Map<ProductDetailDto>(product)).Returns(productDetailDto);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Test Product", result.Name);
            _repositoryMock.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            int productId = 999;
            _repositoryMock.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync((ProductModel?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.Null(result);
            _repositoryMock.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        }
    }
}
