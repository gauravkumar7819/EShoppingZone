using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Profile.API.DTOs;
using EShoppingZone.Profile.API.Services;

namespace EShoppingZone.Profile.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IProfileService _profileService;
        
        public AuthController(IAuthService authService, IProfileService profileService)
        {
            _authService = authService;
            _profileService = profileService;
        }
        
        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = await _profileService.AddCustomerProfileAsync(registerDto);
                return Ok(new { message = "Customer registered successfully", userId = user.Id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpPost("register/merchant")]
        public async Task<IActionResult> RegisterMerchant([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = await _profileService.AddMerchantProfileAsync(registerDto);
                return Ok(new { message = "Merchant registered successfully", userId = user.Id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
        
        [HttpGet("github-login")]
        public IActionResult GitHubLogin()
        {
            var clientId = "your-github-client-id";
            var redirectUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&scope=user:email";
            return Redirect(redirectUrl);
        }
        
        [HttpGet("github-callback")]
        public async Task<IActionResult> GitHubCallback(string code)
        {
            return Ok(new { message = "GitHub OAuth callback received", code });
        }
    }
}