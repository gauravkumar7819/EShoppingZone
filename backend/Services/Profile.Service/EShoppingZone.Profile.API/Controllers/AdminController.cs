using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Profile.API.Repositories;

namespace EShoppingZone.Profile.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IProfileRepository _repository;
        
        public AdminController(IProfileRepository repository)
        {
            _repository = repository;
        }
        
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _repository.GetAllAsync();
            return Ok(users);
        }
        
        [HttpGet("users/role/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            var users = await _repository.GetByRoleAsync(role);
            return Ok(users);
        }
        
        [HttpPut("users/{userId}/activate")]
        public async Task<IActionResult> ActivateUser(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });
                
            user.IsActive = true;
            await _repository.UpdateAsync(user);
            return Ok(new { message = "User activated successfully" });
        }
        
        [HttpPut("users/{userId}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });
                
            user.IsActive = false;
            await _repository.UpdateAsync(user);
            return Ok(new { message = "User deactivated successfully" });
        }
    }
}