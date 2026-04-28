using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Wallet.API.DTOs
{
    public class WalletDto
    {
        [Required]
        public int WalletId { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal CurrentBalance { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        [Required]
        public bool IsActive { get; set; }
    }
    
    public class CreateWalletDto
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal InitialBalance { get; set; } = 0;
    }
    
    public class AddMoneyDto
    {
        [Required]
        public int WalletId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
    
    public class PaymentRequestDto
    {
        [Required]
        public int WalletId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
    
    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string? TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
    }
    
    public class AddMoneyResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal NewBalance { get; set; }
        public int StatementId { get; set; }
    }
    
    public class StatementDto
    {
        public int StatementId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? OrderId { get; set; }
        public string TransactionRemarks { get; set; } = string.Empty;
        public decimal BalanceAfterTransaction { get; set; }
        public string? RazorpayPaymentId { get; set; }
    }
    
    public class WalletBalanceDto
    {
        public int WalletId { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; } = "INR";
    }
    
    public class TransactionSummaryDto
    {
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public int TotalTransactions { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}