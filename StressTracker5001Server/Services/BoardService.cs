using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Services
{
    public interface IBoardService
    {
        Task<Board?> GetBoardByIdAsync(int boardId, int ownerId);
        Task<List<Board>> GetBoardsByOwnerIdAsync(int ownerId);
        Task<Board> CreateBoardAsync(CreateBoardDto dto, int ownerId);
        Task<Board?> UpdateBoardAsync(int boardId, UpdateBoardDto dto, int ownerId);
        Task<bool> DeleteBoardAsync(int boardId, int ownerId);
    }

    public class BoardService : IBoardService
    {
        private readonly AppDbContext _context;

        public BoardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Board?> GetBoardByIdAsync(int boardId, int ownerId)
        {
            return await _context.Boards
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.Id == boardId && b.OwnerId == ownerId);
        }

        public async Task<List<Board>> GetBoardsByOwnerIdAsync(int ownerId)
        {
            return await _context.Boards
                .Where(b => b.OwnerId == ownerId)
                .Include(b => b.Owner)
                .OrderBy(b => b.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Board> CreateBoardAsync(CreateBoardDto dto, int ownerId)
        {
            var board = new Board
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                OwnerId = ownerId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            return board;
        }

        public async Task<Board?> UpdateBoardAsync(int boardId, UpdateBoardDto dto, int ownerId)
        {
            var board = await GetBoardByIdAsync(boardId, ownerId);
            if (board == null)
            {
                return null;
            }

            board.Name = dto.Name;
            board.Description = dto.Description ?? string.Empty;
            board.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return board;
        }

        public async Task<bool> DeleteBoardAsync(int boardId, int ownerId)
        {
            var board = await GetBoardByIdAsync(boardId, ownerId);
            if (board == null)
            {
                return false;
            }

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
