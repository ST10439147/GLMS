using GLMS.Services;
using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Models;

namespace ST10439147_PRO7311_POE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly LoginDTO _loginDto;      // singleton – tracks session access level

        public UserController(IUserService userService, LoginDTO loginDto)
        {
            _userService = userService;
            _loginDto = loginDto;
        }

        // POST api/user/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var user = await _userService.Login(loginDto);
            if (user == null) return Unauthorized();

            // Persist access level into the singleton so other controllers can check it
            _loginDto.AccessLevel = user.AccessLevel;
            return Ok(user);
        }

        // POST api/user/CreateUpdate/{id}   (id = 0 → create)
        [HttpPost("CreateUpdate/{id}")]
        public async Task<IActionResult> CreateUpdate(User user, int id)
        {
            if (_loginDto.AccessLevel != (int)UserRole.Admin)
                return Ok(DenyAccess("Only Admins can create or update users."));

            var result = await _userService.CreateUpdate(user, id);
            return Ok(result);
        }

        // GET api/user/GetUserRecord/{id}
        [HttpGet("GetUserRecord/{id}")]
        public async Task<IActionResult> GetUserRecord(int id)
        {
            var result = await _userService.GetUserRecord(id);
            return result == null ? NotFound() : Ok(result);
        }

        // GET api/user/GetAllUsers
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            if (_loginDto.AccessLevel != (int)UserRole.Admin &&
                _loginDto.AccessLevel != (int)UserRole.Manager)
                return Ok(DenyAccess("Access Denied – Managers and Admins only."));

            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        // DELETE api/user/DeleteUserRecord/{id}
        [HttpDelete("DeleteUserRecord/{id}")]
        public async Task<IActionResult> DeleteUserRecord(int id)
        {
            if (_loginDto.AccessLevel != (int)UserRole.Admin)
                return Ok(DenyAccess("Only Admins can delete users."));

            var result = await _userService.DeleteUserRecord(id);
            return Ok(result);
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private static UserResponseDTO DenyAccess(string message) => new()
        {
            User = new User { Id = -1 },
            Message = message,
            Status = false
        };
    }
}
