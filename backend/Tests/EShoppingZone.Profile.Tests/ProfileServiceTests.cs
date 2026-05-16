using Moq;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using EShoppingZone.Profile.API.Services;
using EShoppingZone.Profile.API.Repositories;
using EShoppingZone.Profile.API.Entities;
using EShoppingZone.Profile.API.DTOs;
using Xunit;

namespace EShoppingZone.Profile.Tests
{
    public class ProfileServiceTests
    {
        private readonly Mock<IProfileRepository> _repositoryMock;
        private readonly Mock<IPasswordHasher<UserProfile>> _passwordHasherMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            _repositoryMock = new Mock<IProfileRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher<UserProfile>>();
            _mapperMock = new Mock<IMapper>();
            _profileService = new ProfileService(_repositoryMock.Object, _passwordHasherMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetProfileByIdAsync_WhenUserExists_ShouldReturnProfileDto()
        {
            // Arrange
            int userId = 1;
            var user = new UserProfile { Id = userId, FullName = "Test User", Email = "test@example.com" };
            var profileDto = new ProfileDto { Id = userId, FullName = "Test User" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<ProfileDto>(user)).Returns(profileDto);

            // Act
            var result = await _profileService.GetProfileByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test User", result.FullName);
        }

        [Fact]
        public async Task RegisterCustomer_WhenEmailExists_ShouldThrowException()
        {
            // Arrange
            var registerDto = new RegisterDto { Email = "exists@example.com", Password = "Password123", FullName = "Test", MobileNumber = "1234567890" };
            _repositoryMock.Setup(repo => repo.EmailExistsAsync(registerDto.Email)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _profileService.AddCustomerProfileAsync(registerDto));
        }
    }
}
