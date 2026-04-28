namespace EShoppingZone.Cart.API.DTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int MerchantId { get; set; }
    }
    
    public class AddToCartDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string? ImageUrl { get; set; }
        public int MerchantId { get; set; }
    }
    
    public class UpdateCartItemDto
    {
        public int CartId { get; set; }
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
    
    public class CartTotalResponseDto
    {
        public int CartId { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public List<CartItemBreakdownDto> Breakdown { get; set; } = new();
    }
    
    public class CartItemBreakdownDto
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}