using Common.Dto;
using Common.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.LoginAsync(loginDto, ipAddress);

            if (result.IsLockedOut)
            {
                return StatusCode(429, new
                {
                    message = "Too many failed login attempts. Please try again later.",
                    lockoutRemainingSeconds = result.LockoutRemainingSeconds
                });
            }

            if (!result.Success)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(new { token = result.Token, expiresAt = result.ExpiresAt });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "New password must be at least 6 characters." });
            }

            var result = await _authService.ChangePasswordAsync(username, dto);

            return result switch
            {
                ChangePasswordResult.UserNotFound => NotFound(new { message = "User not found." }),
                ChangePasswordResult.InvalidCurrentPassword => BadRequest(new { message = "Current password is incorrect." }),
                _ => Ok(new { message = "Password changed successfully!" })
            };
        }
    }
}