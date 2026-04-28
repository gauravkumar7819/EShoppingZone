using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShoppingZone.Wallet.API.Entities
{
    public enum TransactionType
    {
        CREDIT = 1,
        DEBIT = 2
    }
    
    public class Statement
    {
        [Key]
        public int StatementId { get; set; }
        
        [Required]
        public TransactionType TransactionType { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        public int? OrderId { get; set; }
        
        [MaxLength(500)]
        public string TransactionRemarks { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfterTransaction { get; set; }
        
        public string? RazorpayPaymentId { get; set; }
        
        public string? RazorpayOrderId { get; set; }
        
        // Foreign Key
        public int WalletId { get; set; }
        
        // Navigation property
        [ForeignKey("WalletId")]
        public EWallet Wallet { get; set; } = null!;
    }
}