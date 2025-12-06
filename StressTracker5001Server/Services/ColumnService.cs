using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Services
{
    public interface IColumnService
    {
        Task<Column?> GetColumnByIdAsync(int columnId, int ownerId);
        Task<Column> CreateColumnAsync(int boardId, CreateColumnDto dto, int ownerId);
        Task<Column?> UpdateColumnAsync(int columnId, UpdateColumnDto dto, int ownerId);
        Task<bool> MoveColumnAsync(int columnId, int newPosition, int ownerId);
        Task<bool> DeleteColumnAsync(int columnId, int ownerId);
    }

    public class ColumnService : IColumnService
    {
        private readonly AppDbContext _context;

        public ColumnService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Column?> GetColumnByIdAsync(int columnId, int ownerId)
        {
            return await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.Board.OwnerId == ownerId);
        }

        public async Task<Column> CreateColumnAsync(int boardId, CreateColumnDto dto, int ownerId)
        {
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
            return column;
        }

        public async Task<Column?> UpdateColumnAsync(int columnId, UpdateColumnDto dto, int ownerId)
        {
            var column = await GetColumnByIdAsync(columnId, ownerId);
            if (column == null)
            {
                return null;
            }

            column.Name = dto.Name;
            column.WipLimit = dto.WipLimit;
            column.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return column;
        }

        public async Task<bool> MoveColumnAsync(int columnId, int newPosition, int ownerId)
        {
            var column = await GetColumnByIdAsync(columnId, ownerId);
            if (column == null)
            {
                return false;
            }

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
            return true;
        }

        public async Task<bool> DeleteColumnAsync(int columnId, int ownerId)
        {
            var column = await GetColumnByIdAsync(columnId, ownerId);
            if (column == null)
            {
                return false;
            }

            _context.Columns.Remove(column);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
