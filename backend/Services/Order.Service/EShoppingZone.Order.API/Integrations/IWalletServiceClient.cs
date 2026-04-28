using EShoppingZone.Order.API.DTOs;

namespace EShoppingZone.Order.API.Integrations
{
    public interface IWalletServiceClient
    {
        Task<WalletDeductResponseDto> ProcessPaymentAsync(int walletId, decimal amount, int orderId, string? remarks);
        Task<WalletDto?> GetWalletByCustomerIdAsync(int customerId);
    }
}