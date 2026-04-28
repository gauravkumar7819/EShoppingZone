using Microsoft.EntityFrameworkCore;
using EShoppingZone.Wallet.API.Data;
using EShoppingZone.Wallet.API.Entities;
using EShoppingZone.Wallet.API.DTOs;

namespace EShoppingZone.Wallet.API.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly WalletDbContext _context;
        
        public WalletRepository(WalletDbContext context)
        {
            _context = context;
        }
        
        public async Task<EWallet?> GetWalletByIdAsync(int walletId)
        {
            return await _context.Wallets
                .Include(w => w.Statements)
                .FirstOrDefaultAsync(w => w.WalletId == walletId);
        }
        
        public async Task<EWallet?> GetWalletByCustomerIdAsync(int customerId)
        {
            return await _context.Wallets
                .Include(w => w.Statements)
                .FirstOrDefaultAsync(w => w.CustomerId == customerId);
        }
        
        public async Task<IEnumerable<EWallet>> GetAllWalletsAsync()
        {
            return await _context.Wallets
                .Include(w => w.Statements)
                .ToListAsync();
        }
        
        public async Task<EWallet> CreateWalletAsync(EWallet wallet)
        {
            wallet.CreatedAt = DateTime.UtcNow;
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }
        
        public async Task<EWallet> UpdateWalletAsync(EWallet wallet)
        {
            wallet.UpdatedAt = DateTime.UtcNow;
            _context.Entry(wallet).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return wallet;
        }
        
        public async Task<bool> DeleteWalletAsync(int walletId)
        {
            var wallet = await GetWalletByIdAsync(walletId);
            if (wallet == null) return false;
            
            wallet.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> WalletExistsAsync(int walletId)
        {
            return await _context.Wallets.AnyAsync(w => w.WalletId == walletId);
        }
        
        public async Task<bool> CustomerHasWalletAsync(int customerId)
        {
            return await _context.Wallets.AnyAsync(w => w.CustomerId == customerId);
        }
        
        public async Task<Statement> AddStatementAsync(Statement statement)
        {
            _context.Statements.Add(statement);
            await _context.SaveChangesAsync();
            return statement;
        }
        
        public async Task<IEnumerable<Statement>> GetStatementsByWalletIdAsync(int walletId, int pageNumber = 1, int pageSize = 20)
        {
            return await _context.Statements
                .Where(s => s.WalletId == walletId)
                .OrderByDescending(s => s.TransactionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Statement>> GetStatementsByDateRangeAsync(int walletId, DateTime startDate, DateTime endDate)
        {
            return await _context.Statements
                .Where(s => s.WalletId == walletId && 
                           s.TransactionDate >= startDate && 
                           s.TransactionDate <= endDate)
                .OrderByDescending(s => s.TransactionDate)
                .ToListAsync();
        }
        
        public async Task<Statement?> GetStatementByOrderIdAsync(int orderId)
        {
            return await _context.Statements
                .FirstOrDefaultAsync(s => s.OrderId == orderId);
        }
        
        public async Task<Statement?> GetStatementByRazorpayPaymentIdAsync(string paymentId)
        {
            return await _context.Statements
                .FirstOrDefaultAsync(s => s.RazorpayPaymentId == paymentId);
        }
        
        public async Task<TransactionSummaryDto> GetTransactionSummaryAsync(int walletId)
        {
            var statements = await _context.Statements
                .Where(s => s.WalletId == walletId)
                .ToListAsync();
            
            var totalCredits = statements
                .Where(s => s.TransactionType == TransactionType.CREDIT)
                .Sum(s => s.Amount);
            
            var totalDebits = statements
                .Where(s => s.TransactionType == TransactionType.DEBIT)
                .Sum(s => s.Amount);
            
            var wallet = await GetWalletByIdAsync(walletId);
            
            return new TransactionSummaryDto
            {
                TotalCredits = totalCredits,
                TotalDebits = totalDebits,
                TotalTransactions = statements.Count,
                CurrentBalance = wallet?.CurrentBalance ?? 0
            };
        }
    }
}