using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EShoppingZone.Product.API.Data;
using Xunit;

namespace EShoppingZone.Product.Tests
{
    // Note: Program class must be public or use [assembly: InternalsVisibleTo]
    public class ProductApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly HttpClient _client;

        public ProductApiTests(WebApplicationFactory<Program> factory)
        {
            // Set environment variables for the test process
            Environment.SetEnvironmentVariable("JWT:Secret", "test-secret-key-at-least-32-characters-long!!");
            Environment.SetEnvironmentVariable("JWT:Issuer", "TestIssuer");
            Environment.SetEnvironmentVariable("JWT:Audience", "TestAudience");

            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace real DB with InMemory for Integration tests
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddDbContext<ProductDbContext>(opt => opt.UseInMemoryDatabase("TestDoc"));
                });
            }).CreateClient();
        }
        
        // Clean up environment variables to avoid side effects
        public void Dispose()
        {
            Environment.SetEnvironmentVariable("JWT:Secret", null);
            Environment.SetEnvironmentVariable("JWT:Issuer", null);
            Environment.SetEnvironmentVariable("JWT:Audience", null);
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/Product");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
