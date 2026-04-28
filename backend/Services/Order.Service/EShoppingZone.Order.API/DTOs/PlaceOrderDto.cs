using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Order.API.DTOs
{
    public class PlaceOrderDto
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int CartId { get; set; }
        
        [Required]
        public int DeliveryAddressId { get; set; }
        
        [Required]
        public string ModeOfPayment { get; set; } = string.Empty; // COD or Wallet
        
        public string? DeliveryAddress { get; set; }
    }
    
    public class PlaceOrderResponseDto
    {
        public bool Success { get; set; }                 //  overall API success
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty; // Order status
        public string Message { get; set; } = string.Empty;

        public bool PaymentSuccess { get; set; }          //  payment specific
        public string? TransactionId { get; set; }
    }
    
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public int MerchantId { get; set; }
    }
    
    public class CartResponseDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
    
    public class WalletDeductRequestDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = "DEBIT";
        public string Description { get; set; } = string.Empty;
    }
    
    public class WalletDeductResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string? TransactionId { get; set; }
    }
    
    public class WalletDto
    {
        public int WalletId { get; set; }
        public int CustomerId { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class AddressResponseDto
    {
        public int Id { get; set; }
        public string HouseNumber { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string ColonyName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string Landmark { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}