using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface IColumnService
    {
        Task<Result<Column>> GetColumnByIdAsync(int columnId, int ownerId);
        Task<Result<Column>> CreateColumnAsync(int boardId, CreateColumnDto dto, int ownerId);
        Task<Result<Column>> UpdateColumnAsync(int columnId, UpdateColumnDto dto, int ownerId);
        Task<Result<Column>> MoveColumnAsync(int columnId, int newPosition, int ownerId);
        Task<Result<bool>> DeleteColumnAsync(int columnId, int ownerId);
    }

    public class ColumnService : IColumnService
    {
        private readonly AppDbContext _context;

        public ColumnService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Column>> GetColumnByIdAsync(int columnId, int ownerId)
        {
            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.Board.OwnerId == ownerId);

            if (column == null)
            {
                return Result<Column>.NotFound($"Column with ID {columnId} not found or access denied");
            }

            return Result<Column>.Success(column);
        }

        public async Task<Result<Column>> CreateColumnAsync(int boardId, CreateColumnDto dto, int ownerId)
        {
            // Validate board exists and user has access
            var board = await _context.Boards.FindAsync(boardId);
            if (board == null || board.OwnerId != ownerId)
            {
                return Result<Column>.NotFound($"Board with ID {boardId} not found or access denied");
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
            return Result<Column>.Success(column);
        }

        public async Task<Result<Column>> UpdateColumnAsync(int columnId, UpdateColumnDto dto, int ownerId)
        {
            var columnResult = await GetColumnByIdAsync(columnId, ownerId);
            if (!columnResult.IsSuccess)
            {
                return columnResult;
            }

            var column = columnResult.Value!;
            column.Name = dto.Name;
            column.WipLimit = dto.WipLimit;
            column.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<Column>.Success(column);
        }

        public async Task<Result<Column>> MoveColumnAsync(int columnId, int newPosition, int ownerId)
        {
            var columnResult = await GetColumnByIdAsync(columnId, ownerId);
            if (!columnResult.IsSuccess)
            {
                return Result<Column>.NotFound(columnResult.Error ?? "Column not found");
            }

            var column = columnResult.Value!;
            column.Position = newPosition;
            column.UpdatedAt = DateTime.UtcNow;

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
            return Result<Column>.Success(column);
        }

        public async Task<Result<bool>> DeleteColumnAsync(int columnId, int ownerId)
        {
            var columnResult = await GetColumnByIdAsync(columnId, ownerId);
            if (!columnResult.IsSuccess)
            {
                return Result<bool>.NotFound(columnResult.Error ?? "Column not found");
            }

            var column = columnResult.Value!;
            _context.Columns.Remove(column);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
