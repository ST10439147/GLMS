using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE_Back.DTOs;
using ST10439147_PRO7311_POE_Back.Models;
using ST10439147_PRO7311_POE_Back.Services;

namespace ST10439147_PRO7311_POE_Back.Controllers
{
    /// <summary>
    /// Manages user authentication and user records.
    /// Creating, updating, and deleting users requires Admin role.
    /// Listing all users requires Manager or Admin role.
    /// Note: full role-based auth will be expanded in Part 3.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly LoginDTO _loginDto;

        public UserController(IUserService userService, LoginDTO loginDto)
        {
            _userService = userService;
            _loginDto = loginDto;
        }

        /// <summary>
        /// Authenticates a user and returns their profile including access level.
        /// </summary>
        /// <param name="loginDto">Username and password credentials.</param>
        [AllowAnonymous]
        [HttpPost("login")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                var user = await _userService.Login(loginDto);
                if (user == null) return Unauthorized();

                _loginDto.AccessLevel = user.AccessLevel;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns all user records. Requires Manager or Admin role.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            if (_loginDto.AccessLevel != (int)UserRole.Admin &&
                _loginDto.AccessLevel != (int)UserRole.Manager)
                return Ok(DenyAccess("Access Denied – Managers and Admins only."));

            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        /// <summary>Returns a single user record by ID.</summary>
        /// <param name="id">The user ID.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetUserRecord(id);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Creates a new user. Requires Admin role.
        /// </summary>
        /// <param name="user">New user data. Password is required for new users.</param>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            if (_loginDto.AccessLevel != (int)UserRole.Admin)
                return Ok(DenyAccess("Only Admins can create users."));

            var result = await _userService.CreateUpdate(user, 0);
            return Ok(result);
        }

        /// <summary>
        /// Partially updates a user — only the supplied fields are changed. Requires Admin role.
        /// </summary>
        /// <param name="id">The user ID to patch.</param>
        /// <param name="patch">Fields to update. Omit any field to leave it unchanged.</param>
        [HttpPatch("{id:int}")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Patch(int id, [FromBody] PatchUserDto patch)
        {
            if (_loginDto.AccessLevel != (int)UserRole.Admin)
                return Ok(DenyAccess("Only Admins can update users."));

            var result = await _userService.PatchUser(id, patch);
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing user record. Requires Admin role.
        /// </summary>
        /// <param name="id">The user ID to update.</param>
        /// <param name="user">Updated user data.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, [FromBody] User user)
        {
            if (_loginDto.AccessLevel != (int)UserRole.Admin)
                return Ok(DenyAccess("Only Admins can update users."));

            var result = await _userService.CreateUpdate(user, id);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a user record. Requires Admin role.
        /// </summary>
        /// <param name="id">The user ID to delete.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            if (_loginDto.AccessLevel != (int)UserRole.Admin)
                return Ok(DenyAccess("Only Admins can delete users."));

            var result = await _userService.DeleteUserRecord(id);
            return Ok(result);
        }

        private static UserResponseDTO DenyAccess(string message) => new()
        {
            User = new User { Id = -1 },
            Message = message,
            Status = false
        };
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------