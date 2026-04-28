using EShoppingZone.Wallet.API.Entities;
using EShoppingZone.Wallet.API.DTOs;

namespace EShoppingZone.Wallet.API.Repositories
{
    public interface IWalletRepository
    {
        Task<EWallet?> GetWalletByIdAsync(int walletId);
        Task<EWallet?> GetWalletByCustomerIdAsync(int customerId);
        Task<IEnumerable<EWallet>> GetAllWalletsAsync();
        Task<EWallet> CreateWalletAsync(EWallet wallet);
        Task<EWallet> UpdateWalletAsync(EWallet wallet);
        Task<bool> DeleteWalletAsync(int walletId);
        Task<bool> WalletExistsAsync(int walletId);
        Task<bool> CustomerHasWalletAsync(int customerId);
        
        // Statement methods
        Task<Statement> AddStatementAsync(Statement statement);
        Task<IEnumerable<Statement>> GetStatementsByWalletIdAsync(int walletId, int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<Statement>> GetStatementsByDateRangeAsync(int walletId, DateTime startDate, DateTime endDate);
        Task<Statement?> GetStatementByOrderIdAsync(int orderId);
        Task<Statement?> GetStatementByRazorpayPaymentIdAsync(string paymentId);
        Task<TransactionSummaryDto> GetTransactionSummaryAsync(int walletId);
    }
}