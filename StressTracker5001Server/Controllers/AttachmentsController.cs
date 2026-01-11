using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.Extensions;
using StressTracker5001Server.Services;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttachmentsController : ControllerBase
    {
        [Authorize]
        [HttpPost("cards/{cardId}")]
        public async Task<IActionResult> UploadAttachment(
            int cardId,
            IFormFile file,
            [FromServices] IAttachmentService attachmentService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await attachmentService.UploadAttachmentAsync(cardId, file, userId);
            return result.ToActionResult();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttachment(
            Guid id,
            [FromServices] IAttachmentService attachmentService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await attachmentService.GetAttachmentAsync(id, userId);
            return result.ToActionResult();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(
            Guid id,
            [FromServices] IAttachmentService attachmentService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await attachmentService.DeleteAttachmentAsync(id, userId);
            return result.ToActionResult();
        }

        [Authorize]
        [HttpGet("cards/{cardId}")]
        public async Task<IActionResult> GetCardAttachments(
            int cardId,
            [FromServices] IAttachmentService attachmentService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await attachmentService.GetCardAttachmentsAsync(cardId, userId);
            return result.ToActionResult();
        }
    }
}
