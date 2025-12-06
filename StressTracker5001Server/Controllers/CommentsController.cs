using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Services;

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
                return Unauthorized();
            }

            var comment = await commentService.UpdateCommentAsync(commentId, dto, userId);
            if (comment == null)
            {
                return NotFound();
            }

            return Ok(new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = comment.UserId,
                User = new UserDto
                {
                    Id = comment.User.Id,
                    Email = comment.User.Email,
                    Username = comment.User.Username,
                    CreatedAt = comment.User.CreatedAt,
                    UpdatedAt = comment.User.UpdatedAt,
                },
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            });
        }

        [Authorize]
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId, [FromServices] ICommentService commentService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var result = await commentService.DeleteCommentAsync(commentId, userId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}