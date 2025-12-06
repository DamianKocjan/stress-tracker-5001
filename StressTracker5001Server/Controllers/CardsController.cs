using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.DTOs.User;
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
                CreatedBy = new UserDto
                {
                    Id = card.CreatedBy.Id,
                    Email = card.CreatedBy.Email,
                    Username = card.CreatedBy.Username,
                    CreatedAt = card.CreatedBy.CreatedAt,
                    UpdatedAt = card.CreatedBy.UpdatedAt,
                },
                Tags = card.CardTags.Select(ct => ct.TagId).ToList(),
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
        [HttpPost("{id}/move")]
        public async Task<IActionResult> MoveCard(int id, [FromBody] MoveCardDto dto, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var success = await cardService.MoveCardAsync(id, dto, userId);
            if (!success)
            {
                return NotFound();
            }

            var card = await cardService.GetCardByIdAsync(id, userId);
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
        [HttpPost("{id}/tags")]
        public async Task<IActionResult> AssignTagsToCard(int id, [FromBody] CardAssignTagsDto dto, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var result = await cardService.AssignTagsToCardAsync(id, dto.Tags, userId);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [Authorize]
        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetCardComments(int id, [FromServices] ICardService cardService, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                pageSize = 10;
            }

            var comments = await cardService.GetCommentsByCardIdAsync(id, userId, page, pageSize);
            if (comments == null)
            {
                return NotFound();
            }

            var hasMore = await cardService.HasMoreCommentsAsync(id, userId, page, pageSize);

            var commentDtos = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                UserId = c.UserId,
                User = new UserDto
                {
                    Id = c.User.Id,
                    Email = c.User.Email,
                    Username = c.User.Username,
                    CreatedAt = c.User.CreatedAt,
                    UpdatedAt = c.User.UpdatedAt,
                },
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return Ok(new PagedResultDto<CommentDto>
            {
                Items = commentDtos,
                HasMore = hasMore,
                PreviousPage = page > 1 ? page - 1 : 1,
                Page = page,
                NextPage = hasMore ? page + 1 : page,
                PageSize = pageSize
            });
        }

        [Authorize]
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddCommentToCard(int id, [FromBody] CreateCommentDto dto, [FromServices] ICardService cardService, [FromServices] ICommentService commentService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var commentId = await cardService.AddCommentToCardAsync(id, dto, userId);
            if (commentId == null)
            {
                return NotFound();
            }

            var comment = await commentService.GetCommentByIdAsync(commentId.Value, userId);
            if (comment == null)
            {
                return NotFound();
            }

            return Ok(new CommentDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                User = new UserDto
                {
                    Id = comment.User.Id,
                    Email = comment.User.Email,
                    Username = comment.User.Username,
                    CreatedAt = comment.User.CreatedAt,
                    UpdatedAt = comment.User.UpdatedAt,
                },
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
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
