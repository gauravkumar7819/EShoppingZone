using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        
        public AuthController(IAuthService authService, IProfileService profileService, IConfiguration configuration)
        {
            _authService = authService;
            _profileService = profileService;
            _configuration = configuration;
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
        
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var clientId = _configuration["Google:ClientId"]?.Trim();
            if (string.IsNullOrEmpty(clientId))
            {
                return BadRequest(new { error = "Google ClientId is not configured" });
            }

            var redirectUri = _configuration["Google:RedirectUri"]?.Trim() ?? "http://localhost:5173/google-callback";
            var scope = "openid profile email";
            var redirectUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={Uri.EscapeDataString(scope)}&access_type=offline&prompt=consent";
            return Redirect(redirectUrl);
        }
        
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest(new { error = "Authorization code is missing" });
                }

                var clientId = _configuration["Google:ClientId"]?.Trim();
                var clientSecret = _configuration["Google:ClientSecret"]?.Trim();
                var redirectUri = _configuration["Google:RedirectUri"]?.Trim() ?? "http://localhost:5173/google-callback";

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return BadRequest(new { error = "Google settings are incomplete" });
                }
                
                // Exchange code for tokens
                using var httpClient = new HttpClient();
                var tokenRequest = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequest));
                
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                    return BadRequest(new { error = $"Failed to exchange code: {errorContent}", details = "Mismatch usually happens if RedirectUri doesn't match Google Console EXACTLY." });
                }
                
                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                using var tokenDoc = System.Text.Json.JsonDocument.Parse(tokenContent);
                var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    return BadRequest(new { error = "Failed to obtain access token" });
                }

                // Get user info from Google
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var userInfoResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                
                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    var errorContent = await userInfoResponse.Content.ReadAsStringAsync();
                    return BadRequest(new { error = $"Failed to get user info: {errorContent}" });
                }
                
                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                using var userInfoDoc = System.Text.Json.JsonDocument.Parse(userInfoContent);
                var root = userInfoDoc.RootElement;

                var googleId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : (root.TryGetProperty("sub", out var subProp) ? subProp.GetString() : null);
                var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
                var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "Google User";
                
                if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { error = "Incomplete user information received from Google" });
                }

                var response = await _authService.GoogleLoginAsync(googleId, email, name);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}