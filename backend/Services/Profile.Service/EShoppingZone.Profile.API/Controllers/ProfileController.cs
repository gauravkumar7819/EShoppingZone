using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EShoppingZone.Profile.API.DTOs;
using EShoppingZone.Profile.API.Services;

namespace EShoppingZone.Profile.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        
        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }
        
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
        
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var profile = await _profileService.GetProfileByIdAsync(userId);
            
            if (profile == null)
                return NotFound(new { error = "Profile not found" });
                
            return Ok(profile);
        }
        
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProfileById(int id)
        {
            var profile = await _profileService.GetProfileByIdAsync(id);
            
            if (profile == null)
                return NotFound(new { error = "Profile not found" });
                
            return Ok(profile);
        }
        
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateDto)
        {
            try
            {
                var userId = GetUserId();
                var user = await _profileService.UpdateProfileAsync(userId, updateDto);
                return Ok(new { message = "Profile updated successfully", userId = user.Id });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
        
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = GetUserId();
                await _profileService.ChangePasswordAsync(userId, changePasswordDto);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
        
        [HttpPost("address")]
        public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto addressDto)
        {
            try
            {
                var userId = GetUserId();
                var address = await _profileService.AddAddressAsync(userId, addressDto);
                return Ok(new { message = "Address added successfully", addressId = address.Id });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
        
        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = GetUserId();
            var addresses = await _profileService.GetUserAddressesAsync(userId);
            return Ok(addresses);
        }
        
        [HttpPut("address/{addressId}")]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] CreateAddressDto addressDto)
        {
            var result = await _profileService.UpdateAddressAsync(addressId, addressDto);
            if (!result)
                return NotFound(new { error = "Address not found" });
                
            return Ok(new { message = "Address updated successfully" });
        }
        
        [HttpDelete("address/{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var result = await _profileService.DeleteAddressAsync(addressId);
            if (!result)
                return NotFound(new { error = "Address not found" });
                
            return Ok(new { message = "Address deleted successfully" });
        }
        
        [HttpPost("address/{addressId}/default")]
        public async Task<IActionResult> SetDefaultAddress(int addressId)
        {
            var userId = GetUserId();
            await _profileService.SetDefaultAddressAsync(userId, addressId);
            return Ok(new { message = "Default address set successfully" });
        }
        
        [HttpDelete]
        public async Task<IActionResult> DeleteProfile()
        {
            var userId = GetUserId();
            var result = await _profileService.DeleteProfileAsync(userId);
            if (!result)
                return NotFound(new { error = "Profile not found" });
                
            return Ok(new { message = "Profile deleted successfully" });
        }
    }
}