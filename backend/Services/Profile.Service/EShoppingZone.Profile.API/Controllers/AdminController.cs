using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShoppingZone.Profile.API.DTOs;
using EShoppingZone.Profile.API.Repositories;

namespace EShoppingZone.Profile.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IProfileRepository _repository;
        private readonly IMapper _mapper;

        public AdminController(IProfileRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var (users, totalCount) = await _repository.GetAllPaginatedAsync(page, size);
            var dtos = _mapper.Map<IEnumerable<ProfileDto>>(users);
            return Ok(new { items = dtos, totalCount });
        }

        [HttpGet("users/role/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            var users = await _repository.GetByRoleAsync(role);
            var dtos = _mapper.Map<IEnumerable<ProfileDto>>(users);
            return Ok(dtos);
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