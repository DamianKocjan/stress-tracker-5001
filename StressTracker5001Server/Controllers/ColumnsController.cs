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

            dto.ColumnId = columnId;
            var card = await cardService.CreateCardAsync(dto, userId);
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
        [HttpPost]
        public async Task<IActionResult> CreateColumn([FromBody] CreateColumnDto dto, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var column = await columnService.CreateColumnAsync(dto, userId);
            return Ok(new ColumnDto
            {
                Id = column.Id,
                BoardId = column.BoardId,
                Name = column.Name,
                Position = column.Position,
                WipLimit = column.WipLimit,
                CreatedAt = column.CreatedAt,
                UpdatedAt = column.UpdatedAt
            });
        }

        [Authorize]
        [HttpPut("{columnId}")]
        public async Task<IActionResult> UpdateColumn(int columnId, [FromBody] UpdateColumnDto dto, [FromServices] IBoardService boardService, [FromServices] IColumnService columnService)
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
            return Ok(new ColumnDto
            {
                Id = column.Id,
                BoardId = column.BoardId,
                Name = column.Name,
                Position = column.Position,
                WipLimit = column.WipLimit,
                CreatedAt = column.CreatedAt,
                UpdatedAt = column.UpdatedAt
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
