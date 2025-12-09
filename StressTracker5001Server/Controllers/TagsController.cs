using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.Services;
using StressTracker5001Server.Extensions;

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
                return Unauthorized(ResultDto.Unauthorized("Invalid user token"));
            }

            var result = await tagService.CreateTagAsync(dto, userId);
            return result.ToActionResult(t => t.ToDto());
        }

        [Authorize]
        [HttpPut("{tagId}")]
        public async Task<IActionResult> UpdateTag(int tagId, [FromBody] TagUpdateDto dto, [FromServices] ITagService tagService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized(ResultDto.Unauthorized("Invalid user token"));
            }

            var result = await tagService.UpdateTagAsync(tagId, dto, userId);
            return result.ToActionResult(t => t.ToDto());
        }

        [Authorize]
        [HttpDelete("{tagId}")]
        public async Task<IActionResult> DeleteTag(int tagId, [FromServices] ITagService tagService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized(ResultDto.Unauthorized("Invalid user token"));
            }

            var result = await tagService.DeleteTagAsync(tagId, userId);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return result.ToActionResult();
        }
    }
}