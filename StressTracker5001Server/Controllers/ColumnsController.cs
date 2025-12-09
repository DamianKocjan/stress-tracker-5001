using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.Services;
using StressTracker5001Server.Extensions;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ColumnsController : ControllerBase
    {
        [Authorize]
        [HttpPost("{columnId}/cards")]
        public async Task<IActionResult> CreateCardInColumn(int columnId, [FromBody] CreateCardDto dto, [FromServices] ICardService cardService, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var columnResult = await columnService.GetColumnByIdAsync(columnId, userId);
            if (!columnResult.IsSuccess)
            {
                return columnResult.ToActionResult();
            }

            var result = await cardService.CreateCardAsync(columnId, dto, userId);
            return result.ToActionResult(c => c.ToDto());
        }

        [Authorize]
        [HttpPut("{columnId}")]
        public async Task<IActionResult> UpdateColumn(int columnId, [FromBody] UpdateColumnDto dto, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await columnService.UpdateColumnAsync(columnId, dto, userId);
            return result.ToActionResult(c => c.ToDto());
        }

        [Authorize]
        [HttpPost("{columnId}/move")]
        public async Task<IActionResult> MoveColumn(int columnId, [FromBody] MoveColumnDto dto, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await columnService.MoveColumnAsync(columnId, dto.NewPosition, userId);
            return result.ToActionResult(c => c.ToDto());
        }

        [Authorize]
        [HttpDelete("{columnId}")]
        public async Task<IActionResult> DeleteColumn(int columnId, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await columnService.DeleteColumnAsync(columnId, userId);
            if (result.IsSuccess)
            {
                return new ObjectResult(ResultDto.CreateSuccess(204)) { StatusCode = 204 };
            }

            return result.ToActionResult();
        }
    }
}
