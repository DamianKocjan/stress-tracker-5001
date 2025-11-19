using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Auth;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Services;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, [FromServices] IUserService userService, [FromServices] ITokenService tokenService)
        {
            var user = await userService.GetUserByEmailAsync(dto.Email);
            if (user == null || !userService.VerifyPassword(user, dto.Password))
            {
                return Unauthorized();
            }

            var token = tokenService.GenerateToken(user.Id, user.Email, user.Username);
            var refreshToken = tokenService.GenerateRefreshToken();

            await userService.SaveRefreshTokenAsync(user.Id, refreshToken);

            return Ok(new { token, refreshToken });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, [FromServices] IUserService userService)
        {
            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            var user = await userService.CreateUserAsync(new CreateUserDto
            {
                Email = dto.Email,
                Username = dto.Username,
                Password = dto.Password
            });

            if (user == null)
            {
                return BadRequest(new { message = "User registration failed" });
            }
            return Ok();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto, [FromServices] ITokenService tokenService, [FromServices] IUserService userService)
        {
            if (string.IsNullOrEmpty(dto.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            var refreshToken = await tokenService.GetRefreshTokenAsync(dto.RefreshToken);
            if (refreshToken == null)
            {
                return Unauthorized();
            }

            var user = await userService.GetUserByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                return Unauthorized();
            }

            await tokenService.RevokeRefreshTokenAsync(dto.RefreshToken);

            var newToken = tokenService.GenerateToken(user.Id, user.Email, user.Username);
            var newRefreshToken = tokenService.GenerateRefreshToken();

            await userService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

            return Ok(new { token = newToken, refreshToken = newRefreshToken.Token });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile([FromServices] IUserService userService)
        {
            var userId = int.Parse(ClaimTypes.NameIdentifier);
            var user = await userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                user.Id,
                user.Email,
                user.Username,
                user.CreatedAt,
                user.UpdatedAt,
            });
        }

        [Authorize]
        [HttpPost("profile/update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto, [FromServices] IUserService userService)
        {
            var userId = int.Parse(ClaimTypes.NameIdentifier);
            var updatedUser = await userService.UpdateUserAsync(userId, dto);
            if (updatedUser == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                updatedUser.Id,
                updatedUser.Email,
                updatedUser.Username,
                updatedUser.CreatedAt,
                updatedUser.UpdatedAt,
            });
        }

        [Authorize]
        [HttpPost("profile/update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto, [FromServices] IUserService userService)
        {
            var userId = int.Parse(ClaimTypes.NameIdentifier);
            var user = await userService.GetUserByIdAsync(userId);
            if (user == null || !userService.VerifyPassword(user, dto.CurrentPassword))
            {
                return Unauthorized();
            }

            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return BadRequest(new { message = "New passwords do not match" });
            }

            var updatedUser = await userService.UpdateUserPasswordAsync(userId, dto.NewPassword);
            if (updatedUser == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                updatedUser.Id,
                updatedUser.Email,
                updatedUser.Username,
                updatedUser.CreatedAt,
                updatedUser.UpdatedAt,
            });
        }
    }
}
