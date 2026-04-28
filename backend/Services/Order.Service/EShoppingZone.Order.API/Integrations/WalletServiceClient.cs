using System.Text;
using System.Text.Json;
using EShoppingZone.Order.API.DTOs;

namespace EShoppingZone.Order.API.Integrations
{
    public class WalletServiceClient : IWalletServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WalletServiceClient> _logger;
        
        public WalletServiceClient(HttpClient httpClient, ILogger<WalletServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        
        public async Task<WalletDeductResponseDto> ProcessPaymentAsync(int walletId, decimal amount, int orderId, string? remarks)
        {
            try
            {
                var paymentRequest = new
                {
                    WalletId = walletId,
                    Amount = amount,
                    OrderId = orderId,
                    Remarks = remarks
                };
                
                var json = JsonSerializer.Serialize(paymentRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("/api/Wallet/pay", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var paymentResponse = JsonSerializer.Deserialize<WalletDeductResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return paymentResponse ?? new WalletDeductResponseDto { Success = false, Message = "Failed to process payment" };
                }
                
                _logger.LogWarning("Failed to process payment: {StatusCode}", response.StatusCode);
                return new WalletDeductResponseDto { Success = false, Message = "Payment failed" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for wallet {WalletId}", walletId);
                return new WalletDeductResponseDto { Success = false, Message = ex.Message };
            }
        }
        
        public async Task<WalletDto?> GetWalletByCustomerIdAsync(int customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Wallet/customer/{customerId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<WalletDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet for customer {CustomerId}", customerId);
                return null;
            }
        }
    }
}