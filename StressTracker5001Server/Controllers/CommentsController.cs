using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.Services;
using StressTracker5001Server.Extensions;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        [Authorize]
        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDto dto, [FromServices] ICommentService commentService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await commentService.UpdateCommentAsync(commentId, dto, userId);
            return result.ToActionResult(c => c.ToDto());
        }

        [Authorize]
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId, [FromServices] ICommentService commentService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await commentService.DeleteCommentAsync(commentId, userId);
            if (result.IsSuccess)
            {
                return new ObjectResult(ResultDto.CreateSuccess(204)) { StatusCode = 204 };
            }

            return result.ToActionResult();
        }
    }
}