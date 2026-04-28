using Razorpay.Api;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using EShoppingZone.Wallet.API.DTOs;
using System.Net;

namespace EShoppingZone.Wallet.API.Services
{
    public class RazorpayService : IRazorpayService
    {
        private readonly IConfiguration _configuration;
        private readonly IWalletService _walletService;
        private readonly ILogger<RazorpayService> _logger;
        private readonly RazorpayClient _razorpayClient;

        public RazorpayService(
            IConfiguration configuration,
            IWalletService walletService,
            ILogger<RazorpayService> logger)
        {
            _configuration = configuration;
            _walletService = walletService;
            _logger = logger;

            var keyId = _configuration["Razorpay:KeyId"] ?? throw new Exception("Razorpay KeyId not configured");
            var keySecret = _configuration["Razorpay:KeySecret"] ?? throw new Exception("Razorpay KeySecret not configured");

            _razorpayClient = new RazorpayClient(keyId, keySecret);
        }

public async Task<RazorpayOrderResponseDto> CreateOrderAsync(CreateRazorpayOrderDto createOrderDto)
{
    try
    {
        var options = new Dictionary<string, object>
        {
            { "amount", createOrderDto.Amount },
            { "currency", createOrderDto.Currency },
            { "receipt", createOrderDto.Receipt },
            { "notes", createOrderDto.Notes }
        };

        Order order = _razorpayClient.Order.Create(options);

        var response = new RazorpayOrderResponseDto
        {
            Id = order["id"]?.ToString(),
            Entity = order["entity"]?.ToString(),
            Amount = Convert.ToInt32(order["amount"]),
            AmountPaid = Convert.ToInt32(order["amount_paid"]),
            AmountDue = Convert.ToInt32(order["amount_due"]),
            Currency = order["currency"]?.ToString(),
            Receipt = order["receipt"]?.ToString(),
            Status = order["status"]?.ToString(),
            Attempts = Convert.ToInt32(order["attempts"]),
            CreatedAt = Convert.ToInt32(order["created_at"])
        };

        if (order["notes"] is IDictionary<string, object> notes)
        {
            response.Notes = notes.ToDictionary(
                k => k.Key,
                v => v.Value?.ToString() ?? string.Empty
            );
        }

        _logger.LogInformation("Razorpay order created: {OrderId} for amount {Amount}", response.Id, response.Amount);

        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create Razorpay order");
        throw;
    }
}        public async Task<bool> VerifyPaymentSignatureAsync(string orderId, string paymentId, string signature)
        {
            try
            {
                var keySecret = _configuration["Razorpay:KeySecret"];
                var payload = $"{orderId}|{paymentId}";

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

                var isValid = computedSignature == signature;

                _logger.LogInformation("Signature verification for order {OrderId}: {IsValid}", orderId, isValid);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify payment signature for order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<RazorpayPaymentResponseDto> HandlePaymentSuccessAsync(RazorpayPaymentVerificationDto verificationDto)
        {
            var isValid = await VerifyPaymentSignatureAsync(
                verificationDto.OrderId,
                verificationDto.PaymentId,
                verificationDto.Signature);

            if (!isValid)
            {
                return new RazorpayPaymentResponseDto
                {
                    Success = false,
                    Message = "Invalid payment signature"
                };
            }

            var addMoneyResult = await _walletService.AddMoneyAsync(
                verificationDto.WalletId,
                verificationDto.Amount,
                $"Razorpay payment {verificationDto.PaymentId} - {verificationDto.OrderId}",
                verificationDto.PaymentId);

            if (!addMoneyResult.Success)
            {
                return new RazorpayPaymentResponseDto
                {
                    Success = false,
                    Message = addMoneyResult.Message,
                    PaymentId = verificationDto.PaymentId,
                    OrderId = verificationDto.OrderId
                };
            }

            return new RazorpayPaymentResponseDto
            {
                Success = true,
                Message = "Money added to wallet successfully",
                PaymentId = verificationDto.PaymentId,
                OrderId = verificationDto.OrderId,
                NewBalance = addMoneyResult.NewBalance,
                StatementId = addMoneyResult.StatementId
            };
        }
    }
}