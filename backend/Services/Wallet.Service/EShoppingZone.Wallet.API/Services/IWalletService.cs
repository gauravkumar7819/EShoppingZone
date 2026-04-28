using EShoppingZone.Wallet.API.DTOs;
using EShoppingZone.Wallet.API.Entities;

namespace EShoppingZone.Wallet.API.Services
{
    public interface IWalletService
    {
        Task<WalletDto> CreateWalletAsync(int customerId, decimal initialBalance = 0);
        Task<WalletDto?> GetWalletByIdAsync(int walletId);
        Task<WalletDto?> GetWalletByCustomerIdAsync(int customerId);
        Task<IEnumerable<WalletDto>> GetAllWalletsAsync();
        Task<AddMoneyResponseDto> AddMoneyAsync(int walletId, decimal amount, string? remarks, string? razorpayPaymentId = null);
        Task<PaymentResponseDto> ProcessPaymentAsync(int walletId, decimal amount, int orderId, string? remarks);
        Task<bool> RefundAmountAsync(int walletId, decimal amount, int orderId, string? remarks);
        Task<IEnumerable<StatementDto>> GetStatementsAsync(int walletId, int pageNumber = 1, int pageSize = 20);
        Task<WalletBalanceDto> GetBalanceAsync(int walletId);
        Task<TransactionSummaryDto> GetTransactionSummaryAsync(int walletId);
        Task<bool> DeleteWalletAsync(int walletId);
            Task<bool> ProcessRazorpayPaymentAsync(int walletId, decimal amount, string paymentId, string orderId);
    }
}