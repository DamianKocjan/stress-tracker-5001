using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.Services;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] TagCreateDto dto, [FromServices] ITagService tagService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var tag = await tagService.CreateTagAsync(dto, userId);
            if (tag == null)
            {
                return NotFound();
            }

            return Ok(new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color,
                BoardId = tag.BoardId,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt
            });
        }

        [Authorize]
        [HttpPut("{tagId}")]
        public async Task<IActionResult> UpdateTag(int tagId, [FromBody] TagUpdateDto dto, [FromServices] ITagService tagService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var tag = await tagService.UpdateTagAsync(tagId, dto, userId);
            if (tag == null)
            {
                return NotFound();
            }

            return Ok(new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color,
                BoardId = tag.BoardId,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt
            });
        }

        [Authorize]
        [HttpDelete("{tagId}")]
        public async Task<IActionResult> DeleteTag(int tagId, [FromServices] ITagService tagService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var result = await tagService.DeleteTagAsync(tagId, userId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}