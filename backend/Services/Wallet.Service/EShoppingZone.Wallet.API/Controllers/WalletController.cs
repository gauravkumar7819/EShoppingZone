using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using EShoppingZone.Wallet.API.DTOs;
using EShoppingZone.Wallet.API.Services;

namespace EShoppingZone.Wallet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IRazorpayService _razorpayService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WalletController> _logger;
        
        public WalletController(
            IWalletService walletService,
            IRazorpayService razorpayService,
            IConfiguration configuration,
            ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _razorpayService = razorpayService;
            _configuration = configuration;
            _logger = logger;
        }
        
        [HttpPost("create/{customerId}")]
        public async Task<IActionResult> CreateWallet(int customerId, [FromQuery] decimal initialBalance = 0)
        {
            try
            {
                var wallet = await _walletService.CreateWalletAsync(customerId, initialBalance);
                return Ok(new { message = "Wallet created successfully", wallet });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wallet for customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Failed to create wallet" });
            }
        }
        
        [HttpGet("{walletId}")]
        public async Task<IActionResult> GetWalletById(int walletId)
        {
            var wallet = await _walletService.GetWalletByIdAsync(walletId);
            if (wallet == null)
                return NotFound(new { error = "Wallet not found" });
                
            return Ok(wallet);
        }
        
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetWalletByCustomerId(int customerId)
        {
            var wallet = await _walletService.GetWalletByCustomerIdAsync(customerId);
            if (wallet == null)
                return NotFound(new { error = "Wallet not found for this customer" });
                
            return Ok(wallet);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllWallets()
        {
            var wallets = await _walletService.GetAllWalletsAsync();
            return Ok(wallets);
        }
        
        [HttpPost("addmoney")]
        public async Task<IActionResult> AddMoney([FromBody] AddMoneyDto addMoneyDto)
        {
            try
            {
                if (addMoneyDto.Amount <= 0)
                    return BadRequest(new { error = "Amount must be greater than 0" });
                
                var result = await _walletService.AddMoneyAsync(
                    addMoneyDto.WalletId, 
                    addMoneyDto.Amount, 
                    addMoneyDto.Remarks,
                    null);
                
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding money to wallet {WalletId}", addMoneyDto.WalletId);
                return StatusCode(500, new { error = "Failed to add money" });
            }
        }
        
        [HttpPost("pay")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto paymentRequest)
        {
            try
            {
                if (paymentRequest.Amount <= 0)
                    return BadRequest(new { error = "Amount must be greater than 0" });
                
                var result = await _walletService.ProcessPaymentAsync(
                    paymentRequest.WalletId,
                    paymentRequest.Amount,
                    paymentRequest.OrderId,
                    paymentRequest.Remarks);
                
                if (!result.Success)
                    return BadRequest(result);
                
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment from wallet {WalletId}", paymentRequest.WalletId);
                return StatusCode(500, new { error = "Failed to process payment" });
            }
        }
        
        [HttpPost("razorpay/initiate")]
        public async Task<IActionResult> InitiateRazorpayPayment([FromBody] InitiateRazorpayPaymentDto request)
        {
            try
            {
                if (request.Amount <= 0)
                    return BadRequest(new { error = "Amount must be greater than 0" });
                
                var wallet = await _walletService.GetWalletByIdAsync(request.WalletId);
                if (wallet == null)
                    return NotFound(new { error = "Wallet not found" });
                
                var amountInPaise = (int)(request.Amount * 100);
                var receiptId = $"wallet_{request.WalletId}_{DateTime.UtcNow.Ticks}";
                
                var razorpayOrder = await _razorpayService.CreateOrderAsync(new CreateRazorpayOrderDto
                {
                    Amount = amountInPaise,
                    Currency = "INR",
                    Receipt = receiptId,
                    Notes = new Dictionary<string, string>
                    {
                        { "walletId", request.WalletId.ToString() },
                        { "customerId", wallet.CustomerId.ToString() },
                        { "type", "wallet_recharge" }
                    }
                });
                
                return Ok(new InitiatePaymentResponseDto
                {
                    RazorpayOrderId = razorpayOrder.Id,
                    Amount = amountInPaise,
                    Currency = "INR",
                    KeyId = _configuration["Razorpay:KeyId"],
                    Name = "EShoppingZone Wallet",
                    Description = $"Add ₹{request.Amount} to wallet",
                    Prefill = new Dictionary<string, string>(),
                    Notes = new Dictionary<string, string>
                    {
                        { "walletId", request.WalletId.ToString() }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating Razorpay payment");
                return StatusCode(500, new { error = "Failed to initiate payment" });
            }
        }
        
        [HttpPost("razorpay/verify")]
        public async Task<IActionResult> VerifyRazorpayPayment([FromBody] RazorpayPaymentVerificationDto verificationDto)
        {
            try
            {
                var result = await _razorpayService.HandlePaymentSuccessAsync(verificationDto);
                
                if (!result.Success)
                    return BadRequest(result);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Razorpay payment");
                return StatusCode(500, new { error = "Failed to verify payment" });
            }
        }
        
        [HttpPost("razorpay/webhook")]
        public async Task<IActionResult> RazorpayWebhook()
        {
            try
            {
                var json = await new StreamReader(Request.Body).ReadToEndAsync();
                var razorpaySignature = Request.Headers["X-Razorpay-Signature"].ToString();
                
                var webhookSecret = _configuration["Razorpay:WebhookSecret"];
                
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
                var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLower();
                
                if (computedSignature != razorpaySignature)
                {
                    _logger.LogWarning("Invalid Razorpay webhook signature");
                    return Unauthorized(new { error = "Invalid signature" });
                }
                
                dynamic webhookData = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                string eventType = webhookData.event_;
                
                _logger.LogInformation("Received Razorpay webhook event: {EventType}", eventType);
                
                if (eventType == "payment.captured")
                {
                    var paymentId = webhookData.payload.payment.entity.id.ToString();
                    var orderId = webhookData.payload.payment.entity.order_id.ToString();
                    var amount = webhookData.payload.payment.entity.amount / 100.0m;
                    
                    int walletId = 0;
                    if (webhookData.payload.payment.entity.notes != null && 
                        webhookData.payload.payment.entity.notes.walletId != null)
                    {
                        walletId = int.Parse(webhookData.payload.payment.entity.notes.walletId.ToString());
                    }
                    
                    if (walletId > 0)
                    {
                        await _walletService.AddMoneyAsync(
                            walletId,
                            amount,
                            $"Razorpay payment {paymentId} - Webhook",
                            paymentId);
                    }
                }
                
                return Ok(new { status = "received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Razorpay webhook");
                return StatusCode(500, new { error = "Webhook processing failed" });
            }
        }
        
        [HttpPost("refund")]
        public async Task<IActionResult> RefundAmount([FromBody] PaymentRequestDto refundRequest)
        {
            try
            {
                var result = await _walletService.RefundAmountAsync(
                    refundRequest.WalletId,
                    refundRequest.Amount,
                    refundRequest.OrderId,
                    refundRequest.Remarks ?? "Order cancellation refund");
                
                if (!result)
                    return BadRequest(new { error = "Refund failed or already processed" });
                
                return Ok(new { message = "Refund processed successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund to wallet {WalletId}", refundRequest.WalletId);
                return StatusCode(500, new { error = "Failed to process refund" });
            }
        }
        
        [HttpGet("{walletId}/balance")]
        public async Task<IActionResult> GetBalance(int walletId)
        {
            try
            {
                var balance = await _walletService.GetBalanceAsync(walletId);
                return Ok(balance);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
        
        [HttpGet("{walletId}/statements")]
        public async Task<IActionResult> GetStatements(
            int walletId, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 20)
        {
            var statements = await _walletService.GetStatementsAsync(walletId, pageNumber, pageSize);
            return Ok(statements);
        }
        
        [HttpGet("{walletId}/summary")]
        public async Task<IActionResult> GetTransactionSummary(int walletId)
        {
            try
            {
                var summary = await _walletService.GetTransactionSummaryAsync(walletId);
                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
        
        [HttpDelete("{walletId}")]
        public async Task<IActionResult> DeleteWallet(int walletId)
        {
            try
            {
                var result = await _walletService.DeleteWalletAsync(walletId);
                if (!result)
                    return NotFound(new { error = "Wallet not found" });
                    
                return Ok(new { message = "Wallet deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}