using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.Services;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ColumnsController : ControllerBase
    {
        [Authorize]
        [HttpGet("{columnId}/cards")]
        public async Task<IActionResult> GetColumnCards(int columnId, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var column = await columnService.GetColumnByIdAsync(columnId, userId);
            if (column == null)
            {
                return NotFound();
            }

            var cards = await columnService.GetCardsByColumnIdAsync(columnId, userId);
            var cardDtos = cards.Select(card => new CardDto
            {
                Id = card.Id,
                Title = card.Title,
                Description = card.Description,
                Position = card.Position,
                DueDate = card.DueDate,
                CreatedById = card.CreatedById,
                ColumnId = card.ColumnId,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt
            }).ToList();

            return Ok(cardDtos);
        }

        [Authorize]
        [HttpPost("{columnId}/cards")]
        public async Task<IActionResult> CreateCardInColumn(int columnId, [FromBody] CreateCardDto dto, [FromServices] ICardService cardService, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var column = await columnService.GetColumnByIdAsync(columnId, userId);
            if (column == null)
            {
                return NotFound();
            }

            var card = await cardService.CreateCardAsync(columnId, dto, userId);
            return Ok(new CardDto
            {
                Id = card.Id,
                Title = card.Title,
                Description = card.Description,
                Position = card.Position,
                DueDate = card.DueDate,
                CreatedById = card.CreatedById,
                ColumnId = card.ColumnId,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt
            });
        }

        [Authorize]
        [HttpPut("{columnId}")]
        public async Task<IActionResult> UpdateColumn(int columnId, [FromBody] UpdateColumnDto dto, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var column = await columnService.GetColumnByIdAsync(columnId, userId);
            if (column == null)
            {
                return NotFound();
            }

            await columnService.UpdateColumnAsync(columnId, dto, userId);

            var updatedColumn = await columnService.GetColumnByIdAsync(columnId, userId);
            if (updatedColumn == null)
            {
                return NotFound();
            }

            return Ok(new ColumnDto
            {
                Id = updatedColumn.Id,
                BoardId = updatedColumn.BoardId,
                Name = updatedColumn.Name,
                Position = updatedColumn.Position,
                WipLimit = updatedColumn.WipLimit,
                CreatedAt = updatedColumn.CreatedAt,
                UpdatedAt = updatedColumn.UpdatedAt
            });
        }

        [Authorize]
        [HttpPost("{columnId}/move")]
        public async Task<IActionResult> MoveColumn(int columnId, [FromBody] MoveColumnDto dto, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var column = await columnService.GetColumnByIdAsync(columnId, userId);
            if (column == null)
            {
                return NotFound();
            }

            await columnService.MoveColumnAsync(columnId, dto.NewPosition, userId);

            var movedColumn = await columnService.GetColumnByIdAsync(columnId, userId);
            return Ok(new ColumnDto
            {
                Id = movedColumn.Id,
                BoardId = movedColumn.BoardId,
                Name = movedColumn.Name,
                Position = movedColumn.Position,
                WipLimit = movedColumn.WipLimit,
                CreatedAt = movedColumn.CreatedAt,
                UpdatedAt = movedColumn.UpdatedAt
            });
        }

        [Authorize]
        [HttpDelete("{columnId}")]
        public async Task<IActionResult> DeleteColumn(int columnId, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var column = await columnService.GetColumnByIdAsync(columnId, userId);
            if (column == null)
            {
                return NotFound();
            }

            var success = await columnService.DeleteColumnAsync(columnId, userId);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
