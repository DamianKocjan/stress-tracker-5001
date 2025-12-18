using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Auth;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.Services;
using StressTracker5001Server.Extensions;
using StressTracker5001Server.DTOs.BoardInvite;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardInviteController : ControllerBase
    {
        [Authorize]
        [HttpPost("join")]
        public async Task<IActionResult> JoinBoard([FromBody] BoardInviteDto dto, [FromServices] IBoardInviteService boardInviteService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user")) { StatusCode = 401 };
            }

            var result = await boardInviteService.AcceptInviteAsync(userId, dto.Token);
            return result.ToActionResult();
        }

        [Authorize]
        [HttpPost("{inviteId}")]
        public async Task<IActionResult> RevokeInvite([FromRoute] int inviteId, [FromServices] IBoardInviteService boardInviteService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await boardInviteService.RevokeInviteAsync(inviteId, userId);
            if (result.IsSuccess)
            {
                return new ObjectResult(ResultDto.CreateSuccess(204)) { StatusCode = 204 };
            }

            return result.ToActionResult();
        }
    }
}
