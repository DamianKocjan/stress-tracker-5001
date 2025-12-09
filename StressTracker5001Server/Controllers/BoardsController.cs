using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.Services;
using StressTracker5001Server.Extensions;

namespace StressTracker5001Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardsController : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDto dto, [FromServices] IBoardService boardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var createResult = await boardService.CreateBoardAsync(dto, userId);
            if (!createResult.IsSuccess)
            {
                return createResult.ToActionResult();
            }

            var boardId = createResult.Value;
            var result = await boardService.GetBoardByIdAsync(boardId, userId);
            return result.ToActionResult(b => b.ToDto());
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBoards([FromServices] IBoardService boardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await boardService.GetOwnedBoardsAsync(userId);
            return result.ToActionResult(boards => boards.Select(b => b.ToDto()).ToList());
        }

        [Authorize]
        [HttpGet("{boardId}")]
        public async Task<IActionResult> GetBoard([FromRoute] int boardId, [FromServices] IBoardService boardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await boardService.GetBoardWithColumnsAndCardsAsync(boardId, userId);
            return result.ToActionResult();
        }

        [Authorize]
        [HttpPost("{boardId}/columns")]
        public async Task<IActionResult> CreateBoardColumn([FromRoute] int boardId, [FromBody] CreateColumnDto dto, [FromServices] IBoardService boardService, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var boardResult = await boardService.GetBoardByIdAsync(boardId, userId);
            if (!boardResult.IsSuccess)
            {
                return boardResult.ToActionResult();
            }

            var result = await columnService.CreateColumnAsync(boardId, dto, userId);
            return result.ToActionResult(c => c.ToDto());
        }

        [Authorize]
        [HttpPut("{boardId}")]
        public async Task<IActionResult> UpdateBoard([FromRoute] int boardId, [FromBody] UpdateBoardDto dto, [FromServices] IBoardService boardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await boardService.UpdateBoardAsync(boardId, dto, userId);
            return result.ToActionResult(b => b.ToDto());
        }

        [Authorize]
        [HttpDelete("{boardId}")]
        public async Task<IActionResult> DeleteBoard([FromRoute] int boardId, [FromServices] IBoardService boardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return new ObjectResult(ResultDto.Unauthorized("Invalid user token")) { StatusCode = 401 };
            }

            var result = await boardService.DeleteBoardAsync(boardId, userId);
            if (result.IsSuccess)
            {
                return new ObjectResult(ResultDto.CreateSuccess(204)) { StatusCode = 204 };
            }

            return result.ToActionResult();
        }
    }
}
