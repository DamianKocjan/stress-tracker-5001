using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Services;

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
                return Unauthorized();
            }

            var board = await boardService.CreateBoardAsync(dto, userId);
            return Ok(new BoardDto
            {
                Id = board.Id,
                Name = board.Name,
                Description = board.Description,
                OwnerId = board.OwnerId,
                Owner = new UserDto
                {
                    Id = board.Owner.Id,
                    Email = board.Owner.Email,
                    Username = board.Owner.Username,
                    CreatedAt = board.Owner.CreatedAt,
                    UpdatedAt = board.Owner.UpdatedAt,
                },
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBoards([FromServices] IBoardService boardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var boards = await boardService.GetBoardsByOwnerIdAsync(userId);
            var boardDtos = boards.Select(board => new BoardDto
            {
                Id = board.Id,
                Name = board.Name,
                Description = board.Description,
                OwnerId = board.OwnerId,
                Owner = new UserDto
                {
                    Id = board.Owner.Id,
                    Email = board.Owner.Email,
                    Username = board.Owner.Username,
                    CreatedAt = board.Owner.CreatedAt,
                    UpdatedAt = board.Owner.UpdatedAt,
                },
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt
            }).ToList();

            return Ok(boardDtos);
        }

        [Authorize]
        [HttpGet("{boardId}")]
        public async Task<IActionResult> GetBoard([FromRoute] int boardId, [FromServices] IBoardService boardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var board = await boardService.GetBoardByIdAsync(boardId, userId);
            if (board == null)
            {
                return NotFound();
            }

            return Ok(new BoardDto
            {
                Id = board.Id,
                Name = board.Name,
                Description = board.Description,
                OwnerId = board.OwnerId,
                Owner = new UserDto
                {
                    Id = board.Owner.Id,
                    Email = board.Owner.Email,
                    Username = board.Owner.Username,
                    CreatedAt = board.Owner.CreatedAt,
                    UpdatedAt = board.Owner.UpdatedAt,
                },
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt
            });
        }

        [Authorize]
        [HttpGet("{boardId}/columns")]
        public async Task<IActionResult> GetBoardColumns([FromRoute] int boardId, [FromServices] IBoardService boardService, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var board = await boardService.GetBoardByIdAsync(boardId, userId);
            if (board == null)
            {
                return NotFound();
            }

            var columns = await columnService.GetColumnsByBoardIdAsync(boardId, userId);
            var columnDtos = columns.Select(c => new ColumnDto
            {
                Id = c.Id,
                BoardId = c.BoardId,
                Name = c.Name,
                Position = c.Position,
                WipLimit = c.WipLimit,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return Ok(columnDtos);
        }

        [Authorize]
        [HttpPost("{boardId}/columns")]
        public async Task<IActionResult> CreateBoardColumn([FromRoute] int boardId, [FromBody] CreateColumnDto dto, [FromServices] IBoardService boardService, [FromServices] IColumnService columnService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var board = await boardService.GetBoardByIdAsync(boardId, userId);
            if (board == null)
            {
                return NotFound();
            }

            var columnDto = new CreateColumnDto
            {
                BoardId = boardId,
                Name = dto.Name,
                Position = dto.Position,
                WipLimit = dto.WipLimit,
            };

            var column = await columnService.CreateColumnAsync(columnDto, userId);
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
        [HttpPut("{boardId}")]
        public async Task<IActionResult> UpdateBoard([FromRoute] int boardId, [FromBody] UpdateBoardDto dto, [FromServices] IBoardService boardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var board = await boardService.UpdateBoardAsync(boardId, dto, userId);
            if (board == null)
            {
                return NotFound();
            }

            return Ok(new BoardDto
            {
                Id = board.Id,
                Name = board.Name,
                Description = board.Description,
                OwnerId = board.OwnerId,
                Owner = new UserDto
                {
                    Id = board.Owner.Id,
                    Email = board.Owner.Email,
                    Username = board.Owner.Username,
                    CreatedAt = board.Owner.CreatedAt,
                    UpdatedAt = board.Owner.UpdatedAt,
                },
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt
            });
        }

        [Authorize]
        [HttpDelete("{boardId}")]
        public async Task<IActionResult> DeleteBoard([FromRoute] int boardId, [FromServices] IBoardService boardService)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
            {
                return Unauthorized();
            }

            var success = await boardService.DeleteBoardAsync(boardId, userId);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
