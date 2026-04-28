using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using EShoppingZone.Wallet.API.Data;
using EShoppingZone.Wallet.API.DTOs;
using EShoppingZone.Wallet.API.Entities;
using EShoppingZone.Wallet.API.Repositories;

namespace EShoppingZone.Wallet.API.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _repository;
        private readonly WalletDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WalletService> _logger;
        
        public WalletService(
            IWalletRepository repository,
            WalletDbContext context,
            IMapper mapper,
            ILogger<WalletService> logger)
        {
            _repository = repository;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        
        public async Task<WalletDto> CreateWalletAsync(int customerId, decimal initialBalance = 0)
        {
            var existingWallet = await _repository.GetWalletByCustomerIdAsync(customerId);
            if (existingWallet != null)
                throw new InvalidOperationException($"Customer {customerId} already has a wallet");
            
            var wallet = new EWallet
            {
                CustomerId = customerId,
                CurrentBalance = initialBalance,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            wallet = await _repository.CreateWalletAsync(wallet);
            
            if (initialBalance > 0)
            {
                await AddMoneyWithTransaction(wallet.WalletId, initialBalance, "Initial wallet creation", null);
            }
            
            return _mapper.Map<WalletDto>(wallet);
        }
        
        public async Task<WalletDto?> GetWalletByIdAsync(int walletId)
        {
            var wallet = await _repository.GetWalletByIdAsync(walletId);
            return wallet != null ? _mapper.Map<WalletDto>(wallet) : null;
        }
        
        public async Task<WalletDto?> GetWalletByCustomerIdAsync(int customerId)
        {
            var wallet = await _repository.GetWalletByCustomerIdAsync(customerId);
            return wallet != null ? _mapper.Map<WalletDto>(wallet) : null;
        }
        
        public async Task<IEnumerable<WalletDto>> GetAllWalletsAsync()
        {
            var wallets = await _repository.GetAllWalletsAsync();
            return _mapper.Map<IEnumerable<WalletDto>>(wallets);
        }
        
        public async Task<AddMoneyResponseDto> AddMoneyAsync(int walletId, decimal amount, string? remarks, string? razorpayPaymentId = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");
            
            var wallet = await _repository.GetWalletByIdAsync(walletId);
            if (wallet == null)
                throw new KeyNotFoundException($"Wallet {walletId} not found");
            
            if (!wallet.IsActive)
                throw new InvalidOperationException("Wallet is deactivated");
            
            // Check if payment already processed
            if (!string.IsNullOrEmpty(razorpayPaymentId))
            {
                var existingStatement = await _repository.GetStatementByRazorpayPaymentIdAsync(razorpayPaymentId);
                if (existingStatement != null)
                {
                    return new AddMoneyResponseDto
                    {
                        Success = true,
                        Message = "Payment already processed",
                        NewBalance = wallet.CurrentBalance,
                        StatementId = existingStatement.StatementId
                    };
                }
            }
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                wallet.CurrentBalance += amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                
                await _repository.UpdateWalletAsync(wallet);
                
                var statement = new Statement
                {
                    WalletId = walletId,
                    TransactionType = TransactionType.CREDIT,
                    Amount = amount,
                    TransactionRemarks = remarks ?? "Money added to wallet",
                    BalanceAfterTransaction = wallet.CurrentBalance,
                    TransactionDate = DateTime.UtcNow,
                    RazorpayPaymentId = razorpayPaymentId
                };
                
                statement = await _repository.AddStatementAsync(statement);
                
                await transaction.CommitAsync();
                
                _logger.LogInformation("Added {Amount} to wallet {WalletId}. New balance: {Balance}", 
                    amount, walletId, wallet.CurrentBalance);
                
                return new AddMoneyResponseDto
                {
                    Success = true,
                    Message = "Money added successfully",
                    NewBalance = wallet.CurrentBalance,
                    StatementId = statement.StatementId
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error adding money to wallet {WalletId}", walletId);
                throw;
            }
        }
        
        private async Task<AddMoneyResponseDto> AddMoneyWithTransaction(int walletId, decimal amount, string remarks, string? razorpayPaymentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var wallet = await _repository.GetWalletByIdAsync(walletId);
                if (wallet == null) throw new KeyNotFoundException($"Wallet {walletId} not found");
                
                wallet.CurrentBalance += amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                
                await _repository.UpdateWalletAsync(wallet);
                
                var statement = new Statement
                {
                    WalletId = walletId,
                    TransactionType = TransactionType.CREDIT,
                    Amount = amount,
                    TransactionRemarks = remarks,
                    BalanceAfterTransaction = wallet.CurrentBalance,
                    TransactionDate = DateTime.UtcNow,
                    RazorpayPaymentId = razorpayPaymentId
                };
                
                statement = await _repository.AddStatementAsync(statement);
                
                await transaction.CommitAsync();
                
                return new AddMoneyResponseDto
                {
                    Success = true,
                    Message = "Money added successfully",
                    NewBalance = wallet.CurrentBalance,
                    StatementId = statement.StatementId
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error in AddMoneyWithTransaction for wallet {WalletId}", walletId);
                throw;
            }
        }
        
        public async Task<PaymentResponseDto> ProcessPaymentAsync(int walletId, decimal amount, int orderId, string? remarks)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");
            
            var wallet = await _repository.GetWalletByIdAsync(walletId);
            if (wallet == null)
                throw new KeyNotFoundException($"Wallet {walletId} not found");
            
            if (!wallet.IsActive)
                throw new InvalidOperationException("Wallet is deactivated");
            
            if (wallet.CurrentBalance < amount)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Insufficient funds. Available balance: {wallet.CurrentBalance}",
                    Balance = wallet.CurrentBalance
                };
            }
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                wallet.CurrentBalance -= amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                
                await _repository.UpdateWalletAsync(wallet);
                
                var statement = new Statement
                {
                    WalletId = walletId,
                    TransactionType = TransactionType.DEBIT,
                    Amount = amount,
                    OrderId = orderId,
                    TransactionRemarks = remarks ?? $"Payment for order #{orderId}",
                    BalanceAfterTransaction = wallet.CurrentBalance,
                    TransactionDate = DateTime.UtcNow
                };
                
                statement = await _repository.AddStatementAsync(statement);
                
                await transaction.CommitAsync();
                
                _logger.LogInformation("Processed payment of {Amount} from wallet {WalletId} for order {OrderId}. New balance: {Balance}", 
                    amount, walletId, orderId, wallet.CurrentBalance);
                
                return new PaymentResponseDto
                {
                    Success = true,
                    Message = "Payment processed successfully",
                    Balance = wallet.CurrentBalance,
                    TransactionId = statement.StatementId.ToString(),
                    TransactionDate = statement.TransactionDate
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing payment from wallet {WalletId} for order {OrderId}", walletId, orderId);
                throw;
            }
        }
        
        public async Task<bool> RefundAmountAsync(int walletId, decimal amount, int orderId, string? remarks)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");
            
            var wallet = await _repository.GetWalletByIdAsync(walletId);
            if (wallet == null)
                throw new KeyNotFoundException($"Wallet {walletId} not found");
            
            var existingStatement = await _repository.GetStatementByOrderIdAsync(orderId);
            if (existingStatement != null && existingStatement.TransactionType == TransactionType.CREDIT)
            {
                _logger.LogWarning("Refund already processed for order {OrderId}", orderId);
                return false;
            }
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                wallet.CurrentBalance += amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                
                await _repository.UpdateWalletAsync(wallet);
                
                var statement = new Statement
                {
                    WalletId = walletId,
                    TransactionType = TransactionType.CREDIT,
                    Amount = amount,
                    OrderId = orderId,
                    TransactionRemarks = remarks ?? $"Refund for order #{orderId}",
                    BalanceAfterTransaction = wallet.CurrentBalance,
                    TransactionDate = DateTime.UtcNow
                };
                
                await _repository.AddStatementAsync(statement);
                
                await transaction.CommitAsync();
                
                _logger.LogInformation("Processed refund of {Amount} to wallet {WalletId} for order {OrderId}. New balance: {Balance}", 
                    amount, walletId, orderId, wallet.CurrentBalance);
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing refund to wallet {WalletId} for order {OrderId}", walletId, orderId);
                throw;
            }
        }
        
        public async Task<IEnumerable<StatementDto>> GetStatementsAsync(int walletId, int pageNumber = 1, int pageSize = 20)
        {
            var statements = await _repository.GetStatementsByWalletIdAsync(walletId, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<StatementDto>>(statements);
        }
        
        public async Task<WalletBalanceDto> GetBalanceAsync(int walletId)
        {
            var wallet = await _repository.GetWalletByIdAsync(walletId);
            if (wallet == null)
                throw new KeyNotFoundException($"Wallet {walletId} not found");
            
            return new WalletBalanceDto
            {
                WalletId = walletId,
                CurrentBalance = wallet.CurrentBalance,
                Currency = "INR"
            };
        }
        
        public async Task<TransactionSummaryDto> GetTransactionSummaryAsync(int walletId)
        {
            var wallet = await _repository.GetWalletByIdAsync(walletId);
            if (wallet == null)
                throw new KeyNotFoundException($"Wallet {walletId} not found");
            
            return await _repository.GetTransactionSummaryAsync(walletId);
        }
        
        public async Task<bool> DeleteWalletAsync(int walletId)
        {
            var wallet = await _repository.GetWalletByIdAsync(walletId);
            if (wallet == null) return false;
            
            if (wallet.CurrentBalance > 0)
                throw new InvalidOperationException($"Cannot delete wallet with positive balance: {wallet.CurrentBalance}");
            
            return await _repository.DeleteWalletAsync(walletId);
        }
        
        public async Task<bool> ProcessRazorpayPaymentAsync(int walletId, decimal amount, string paymentId, string orderId)
        {
            var result = await AddMoneyAsync(walletId, amount, $"Razorpay payment {paymentId} for order {orderId}", paymentId);
            return result.Success;
        }
    }
}