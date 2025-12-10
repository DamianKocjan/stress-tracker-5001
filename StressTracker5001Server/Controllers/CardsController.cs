using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.Services;
using StressTracker5001Server.Extensions;

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
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await cardService.GetCardDetailsByIdAsync(id, userId);
            return result.ToActionResult(c => c.ToDetailsDto());
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCard(int id, [FromBody] UpdateCardDto dto, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await cardService.UpdateCardAsync(id, dto, userId);
            return result.ToActionResult(c => c.ToDto());
        }

        [Authorize]
        [HttpPost("{id}/move")]
        public async Task<IActionResult> MoveCard(int id, [FromBody] MoveCardDto dto, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await cardService.MoveCardAsync(id, dto, userId);
            return result.ToActionResult(c => c.ToDto());
        }

        [Authorize]
        [HttpPost("{id}/tags")]
        public async Task<IActionResult> AssignTagsToCard(int id, [FromBody] CardAssignTagsDto dto, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await cardService.AssignTagsToCardAsync(id, dto.Tags, userId);
            return result.ToActionResult(c => c.ToDto());
        }

        [Authorize]
        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetCardComments(int id, [FromServices] ICardService cardService, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                pageSize = 10;
            }

            var commentsResult = await cardService.GetCommentsByCardIdAsync(id, userId, page, pageSize);
            if (!commentsResult.IsSuccess)
            {
                return commentsResult.ToActionResult();
            }

            var comments = commentsResult.Value!;
            var hasMoreResult = await cardService.HasMoreCommentsAsync(id, userId, page, pageSize);
            var hasMore = hasMoreResult.IsSuccess && hasMoreResult.Value;

            var commentDtos = comments.Select(c => c.ToDto()).ToList();

            return new ObjectResult(ResultDto.CreateSuccessResult(new PagedResultDto<CommentDto>
            {
                Items = commentDtos,
                HasMore = hasMore,
                PreviousPage = page > 1 ? page - 1 : 1,
                Page = page,
                NextPage = hasMore ? page + 1 : page,
                PageSize = pageSize
            }));
        }

        [Authorize]
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddCommentToCard(int id, [FromBody] CreateCommentDto dto, [FromServices] ICardService cardService, [FromServices] ICommentService commentService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var commentResult = await cardService.AddCommentToCardAsync(id, dto, userId);
            return commentResult.ToActionResult(c => c.ToDto());
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(int id, [FromServices] ICardService cardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await cardService.DeleteCardAsync(id, userId);
            if (result.IsSuccess)
            {
                return new ObjectResult(ResultDto.CreateSuccess(204)) { StatusCode = 204 };
            }

            return result.ToActionResult();
        }
    }
}
