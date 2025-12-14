using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Auth;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.Services;
using StressTracker5001Server.Extensions;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, [FromServices] IUserService userService, [FromServices] ITokenService tokenService)
        {
            var userResult = await userService.GetUserByEmailAsync(dto.Email);
            if (!userResult.IsSuccess)
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid email or password")) { StatusCode = 401 };
            }

            var user = userResult.Value!;
            if (!userService.VerifyPassword(user, dto.Password))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid email or password")) { StatusCode = 401 };
            }

            var token = tokenService.GenerateToken(user.Id, user.Email, user.Username);
            var refreshToken = tokenService.GenerateRefreshToken();

            await tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

            tokenService.ApplyTokensToResponse(Response, token, refreshToken.Token);

            return Ok(ResultDto.CreateSuccess());
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, [FromServices] IUserService userService)
        {
            var result = await userService.CreateUserAsync(new CreateUserDto
            {
                Email = dto.Email,
                Username = dto.Username,
                Password = dto.Password
            });

            return result.ToActionResult();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromServices] ITokenService tokenService)
        {
            var refreshTokenCookie = tokenService.GetRefreshTokenFromRequest(Request);
            if (!string.IsNullOrEmpty(refreshTokenCookie))
            {
                await tokenService.RevokeRefreshTokenAsync(refreshTokenCookie);
            }

            tokenService.RemoveTokensFromResponse(Response);

            return Ok(ResultDto.CreateSuccess());
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromServices] ITokenService tokenService)
        {
            var token = tokenService.GetTokenFromRequest(Request);
            if (string.IsNullOrEmpty(token))
            {
                return new ObjectResult(ResultDto.Unauthorized("Token is required")) { StatusCode = 401 };
            }

            var isValid = await tokenService.ValidateTokenAsync(token);
            if (!isValid)
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid or expired token")) { StatusCode = 401 };
            }

            return Ok(ResultDto<object>.CreateSuccessResult(new { valid = true }));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromServices] ITokenService tokenService, [FromServices] IUserService userService)
        {
            var refreshTokenCookie = tokenService.GetRefreshTokenFromRequest(Request);
            if (string.IsNullOrEmpty(refreshTokenCookie))
            {
                return BadRequest(ResultDto.CreateFailureResult("Refresh token is required"));
            }

            var refreshToken = await tokenService.GetRefreshTokenAsync(refreshTokenCookie);
            if (!refreshToken.IsSuccess)
            {
                return new ObjectResult(ResultDto.Unauthorized(refreshToken.Error ?? "Invalid or expired refresh token")) { StatusCode = 401 };
            }

            var userResult = await userService.GetUserByIdAsync(refreshToken.Value!.UserId);
            if (!userResult.IsSuccess)
            {
                return new ObjectResult(ResultDto.Unauthorized("User not found")) { StatusCode = 401 };
            }

            var user = userResult.Value!;
            await tokenService.RevokeRefreshTokenAsync(refreshToken.Value!.Token);

            var newToken = tokenService.GenerateToken(user.Id, user.Email, user.Username);
            var newRefreshToken = tokenService.GenerateRefreshToken();

            await tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

            tokenService.ApplyTokensToResponse(Response, newToken, newRefreshToken.Token);

            return Ok(ResultDto.CreateSuccess());
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile([FromServices] IUserService userService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await userService.GetUserByIdAsync(userId);
            return result.ToActionResult(u => u.ToDto());
        }

        [Authorize]
        [HttpPost("profile/update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto, [FromServices] IUserService userService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await userService.UpdateUserAsync(userId, dto);
            return result.ToActionResult(u => u.ToDto());
        }

        [Authorize]
        [HttpPost("profile/update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto, [FromServices] IUserService userService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var userResult = await userService.GetUserByIdAsync(userId);
            if (!userResult.IsSuccess)
            {
                return new ObjectResult(ResultDto.Unauthorized("User not found")) { StatusCode = 401 };
            }

            var user = userResult.Value!;
            if (!userService.VerifyPassword(user, dto.CurrentPassword))
            {
                return new ObjectResult(ResultDto.Unauthorized("Current password is incorrect")) { StatusCode = 401 };
            }

            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return BadRequest(ResultDto.CreateFailureResult("New passwords do not match"));
            }

            var result = await userService.UpdateUserPasswordAsync(userId, dto.NewPassword);
            return result.ToActionResult();
        }
    }
}
