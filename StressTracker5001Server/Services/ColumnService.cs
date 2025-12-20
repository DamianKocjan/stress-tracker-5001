using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface IColumnService
    {
        Task<Result<Column>> GetColumnByIdAsync(int columnId, int userId, BoardMemberRole requiredRole = BoardMemberRole.Viewer);
        Task<Result<Column>> CreateColumnAsync(int boardId, CreateColumnDto dto, int userId);
        Task<Result<Column>> UpdateColumnAsync(int columnId, UpdateColumnDto dto, int userId);
        Task<Result<Column>> MoveColumnAsync(int columnId, int newPosition, int userId);
        Task<Result<bool>> DeleteColumnAsync(int columnId, int userId);
    }

    public class ColumnService : IColumnService
    {
        private readonly AppDbContext _context;
        private readonly IBoardAuthorizationService _boardAuthorizationService;
        private readonly IActivityLogService _activityLogService;

        public ColumnService(AppDbContext context, IBoardAuthorizationService boardAuthorizationService, IActivityLogService activityLogService)
        {
            _context = context;
            _boardAuthorizationService = boardAuthorizationService;
            _activityLogService = activityLogService;
        }

        public async Task<Result<Column>> GetColumnByIdAsync(int columnId, int userId, BoardMemberRole requiredRole = BoardMemberRole.Viewer)
        {
            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == columnId);

            if (column == null)
            {
                return Result<Column>.NotFound($"Column with ID {columnId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(column.BoardId, userId, requiredRole))
            {
                return Result<Column>.Forbidden("You do not have permission to access this column");
            }

            return Result<Column>.Success(column);
        }

        public async Task<Result<Column>> CreateColumnAsync(int boardId, CreateColumnDto dto, int userId)
        {
            // Validate board exists and user has access
            var board = await _context.Boards.FindAsync(boardId);
            if (board == null)
            {
                return Result<Column>.NotFound($"Board with ID {boardId} not found");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Result<Column>.Failure("Column name is required", 400);
            }

            if (dto.Position < 0)
            {
                return Result<Column>.Failure("Position must be greater than or equal to 0", 400);
            }

            if (dto.WipLimit.HasValue && dto.WipLimit.Value < 0)
            {
                return Result<Column>.Failure("WIP limit must be greater than or equal to 0", 400);
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Admin))
            {
                return Result<Column>.Forbidden("You do not have permission to add columns to this board");
            }

            var now = DateTime.UtcNow;

            var column = new Column
            {
                BoardId = boardId,
                Name = dto.Name,
                Position = dto.Position,
                WipLimit = dto.WipLimit,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Columns.Add(column);
            await _context.SaveChangesAsync();

            await _activityLogService.LogColumnCreatedAsync(boardId, userId, column.Id, column.Name);

            return Result<Column>.Success(column);
        }

        public async Task<Result<Column>> UpdateColumnAsync(int columnId, UpdateColumnDto dto, int userId)
        {
            var columnResult = await GetColumnByIdAsync(columnId, userId, BoardMemberRole.Admin);
            if (!columnResult.IsSuccess)
            {
                return columnResult;
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Result<Column>.Failure("Column name is required", 400);
            }

            if (dto.WipLimit.HasValue && dto.WipLimit.Value < 0)
            {
                return Result<Column>.Failure("WIP limit must be greater than or equal to 0", 400);
            }

            var column = columnResult.Value!;
            var oldColumn = new { column.Name, column.WipLimit };

            column.Name = dto.Name;
            column.WipLimit = dto.WipLimit;
            column.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLogService.LogColumnUpdatedAsync(columnId, userId, columnId, oldColumn, new { Name = column.Name, WipLimit = column.WipLimit });

            return Result<Column>.Success(column);
        }

        public async Task<Result<Column>> MoveColumnAsync(int columnId, int newPosition, int userId)
        {
            var columnResult = await GetColumnByIdAsync(columnId, userId, BoardMemberRole.Admin);
            if (!columnResult.IsSuccess)
            {
                return columnResult.StatusCode switch
                {
                    403 => Result<Column>.Forbidden(columnResult.Error ?? "Forbidden"),
                    404 => Result<Column>.NotFound(columnResult.Error ?? "Not found"),
                    _ => Result<Column>.Failure(columnResult.Error ?? "Error", columnResult.StatusCode)
                };
            }

            if (newPosition < 0)
            {
                return Result<Column>.Failure("New position must be greater than or equal to 0", 400);
            }

            var column = columnResult.Value!;
            int oldPosition = column.Position;
            column.Position = newPosition;
            column.UpdatedAt = DateTime.UtcNow;

            var totalColumns = await _context.Columns.CountAsync(c => c.BoardId == column.BoardId);
            if (newPosition >= totalColumns)
            {
                return Result<Column>.Failure("New position is out of range", 400);
            }

            // Get all columns in the same board to adjust their positions
            var columns = await _context.Columns
                .Where(c => c.BoardId == column.BoardId && c.Id != columnId)
                .OrderBy(c => c.Position)
                .ToListAsync();
            columns.Insert(newPosition, column);

            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].Position = i;
                columns[i].UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _activityLogService.LogColumnMovedAsync(column.BoardId, userId, columnId, oldPosition, newPosition);

            return Result<Column>.Success(column);
        }

        public async Task<Result<bool>> DeleteColumnAsync(int columnId, int userId)
        {
            var columnResult = await GetColumnByIdAsync(columnId, userId, BoardMemberRole.Admin);
            if (!columnResult.IsSuccess)
            {
                return columnResult.StatusCode switch
                {
                    403 => Result<bool>.Forbidden(columnResult.Error ?? "Forbidden"),
                    404 => Result<bool>.NotFound(columnResult.Error ?? "Not found"),
                    _ => Result<bool>.Failure(columnResult.Error ?? "Error", columnResult.StatusCode)
                };
            }

            var column = columnResult.Value!;
            var columnName = column.Name;
            _context.Columns.Remove(column);
            await _context.SaveChangesAsync();

            await _activityLogService.LogColumnDeletedAsync(column.BoardId, userId, columnId, columnName);

            return Result<bool>.Success(true);
        }
    }
}
