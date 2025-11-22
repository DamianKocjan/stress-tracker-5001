using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Services
{
    public interface IColumnService
    {
        Task<List<Column>> GetColumnsByBoardIdAsync(int boardId, int ownerId);
        Task<Column?> GetColumnByIdAsync(int columnId, int ownerId);
        Task<List<Card>> GetCardsByColumnIdAsync(int columnId, int ownerId);
        Task<Column> CreateColumnAsync(CreateColumnDto dto, int ownerId);
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

        public async Task<List<Column>> GetColumnsByBoardIdAsync(int boardId, int ownerId)
        {
            return await _context.Columns
                .Include(c => c.Cards)
                .Where(c => c.BoardId == boardId && c.Board.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<Column?> GetColumnByIdAsync(int columnId, int ownerId)
        {
            return await _context.Columns
                .Include(c => c.Cards)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.Board.OwnerId == ownerId);
        }

        public async Task<List<Card>> GetCardsByColumnIdAsync(int columnId, int ownerId)
        {
            return await _context.Cards
                .Where(c => c.ColumnId == columnId && c.Column.Board.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<Column> CreateColumnAsync(CreateColumnDto dto, int ownerId)
        {
            var column = new Column
            {
                BoardId = dto.BoardId,
                Name = dto.Name,
                Position = dto.Position,
                WipLimit = dto.WipLimit,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
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
            column.UpdatedAt = DateTimeOffset.UtcNow;

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
            column.UpdatedAt = DateTimeOffset.UtcNow;

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
