using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.Services;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCard(int id, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var card = await cardService.GetCardDetailsByIdAsync(id, userId);
            if (card == null)
            {
                return NotFound();
            }

            return Ok(new CardDetailsDto
            {
                Id = card.Id,
                Title = card.Title,
                Description = card.Description,
                Position = card.Position,
                DueDate = card.DueDate,
                CreatedById = card.CreatedById,
                CreatedBy = new DTOs.User.UserDto
                {
                    Id = card.CreatedBy.Id,
                    Email = card.CreatedBy.Email,
                    Username = card.CreatedBy.Username,
                    CreatedAt = card.CreatedBy.CreatedAt,
                    UpdatedAt = card.CreatedBy.UpdatedAt,
                },
                ColumnId = card.ColumnId,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCard(int id, [FromBody] UpdateCardDto dto, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var card = await cardService.UpdateCardAsync(id, dto, userId);
            if (card == null)
            {
                return NotFound();
            }

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
        [HttpPut("{id}/move")]
        public async Task<IActionResult> MoveCard(int id, [FromBody] MoveCardDto dto, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var success = await cardService.MoveCardAsync(id, dto.NewPosition, userId);
            if (!success)
            {
                return NotFound();
            }

            var card = await cardService.GetCardByIdAsync(id, userId);
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(int id, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var result = await cardService.DeleteCardAsync(id, userId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
