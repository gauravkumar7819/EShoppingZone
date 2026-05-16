using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EShoppingZone.Gateway.Tests
{
    public class GatewayRoutingTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public GatewayRoutingTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["JWT:Secret"] = "test-secret-key-at-least-32-characters-long!!",
                        ["JWT:Issuer"] = "TestIssuer",
                        ["JWT:Audience"] = "TestAudience"
                    });
                });
            }).CreateClient();
        }

        [Fact]
        public async Task Gateway_ShouldRespond_WhenHittingProfileRoute()
        {
            // Act - Hitting a route that should be handled by YARP
            var response = await _client.GetAsync("/api/profile/health");

            // Assert
            // It might be 502 (Bad Gateway) or 503 (Service Unavailable) 
            // if downstream is not running, but it shouldn't be 404 (if routing works)
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Gateway_ShouldReturn404_WhenHittingInvalidRoute()
        {
            // Act
            var response = await _client.GetAsync("/api/nonexistent/route");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
