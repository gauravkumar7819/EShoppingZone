using EShoppingZone.Wallet.API.DTOs;

namespace EShoppingZone.Wallet.API.Services
{
    public interface IRazorpayService
    {
        Task<RazorpayOrderResponseDto> CreateOrderAsync(CreateRazorpayOrderDto createOrderDto);
        Task<bool> VerifyPaymentSignatureAsync(string orderId, string paymentId, string signature);
        Task<RazorpayPaymentResponseDto> HandlePaymentSuccessAsync(RazorpayPaymentVerificationDto verificationDto);
    }
}