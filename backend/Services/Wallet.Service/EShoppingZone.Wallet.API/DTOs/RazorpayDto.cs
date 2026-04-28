using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Wallet.API.DTOs
{
    public class CreateRazorpayOrderDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; } // in paise
        
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "INR";
        
        [Required]
        [MaxLength(100)]
        public string Receipt { get; set; } = string.Empty;
        
        public Dictionary<string, string> Notes { get; set; } = new();
    }
    
    public class RazorpayOrderResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public int Amount { get; set; }
        public int AmountPaid { get; set; }
        public int AmountDue { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Receipt { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Attempts { get; set; }
        public Dictionary<string, string> Notes { get; set; } = new();
        public int CreatedAt { get; set; }
    }
    
    public class RazorpayPaymentVerificationDto
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;
        
        [Required]
        public string PaymentId { get; set; } = string.Empty;
        
        [Required]
        public string Signature { get; set; } = string.Empty;
        
        [Required]
        public int WalletId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
    
    public class RazorpayPaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public decimal NewBalance { get; set; }
        public int StatementId { get; set; }
    }
    
    public class InitiateRazorpayPaymentDto
    {
        [Required]
        public int WalletId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
    
    public class InitiatePaymentResponseDto
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string KeyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, string> Prefill { get; set; } = new();
        public Dictionary<string, string> Notes { get; set; } = new();
    }
}