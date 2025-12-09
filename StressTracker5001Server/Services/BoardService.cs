using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;
using StressTracker5001Server.Extensions;

namespace StressTracker5001Server.Services
{
    public interface IBoardService
    {
        Task<Result<Board>> GetBoardByIdAsync(int boardId, int userId);
        Task<Result<BoardDetailsDto>> GetBoardWithColumnsAndCardsAsync(int boardId, int userId);
        Task<Result<List<Board>>> GetOwnedBoardsAsync(int userId);
        Task<Result<List<Board>>> GetUserMembershipBoardsAsync(int userId);
        Task<Result<int>> CreateBoardAsync(CreateBoardDto dto, int ownerId);
        Task<Result<Board>> UpdateBoardAsync(int boardId, UpdateBoardDto dto, int userId);
        Task<Result<bool>> DeleteBoardAsync(int boardId, int userId);
    }

    public class BoardService : IBoardService
    {
        private readonly AppDbContext _context;

        public BoardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Board>> GetBoardByIdAsync(int boardId, int userId)
        {
            var board = await _context.Boards
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.Id == boardId &&
                    (b.OwnerId == userId || b.Members.Any(m => m.UserId == userId)));

            if (board == null)
            {
                return Result<Board>.NotFound($"Board with ID {boardId} not found or access denied");
            }

            return Result<Board>.Success(board);
        }

        public async Task<Result<BoardDetailsDto>> GetBoardWithColumnsAndCardsAsync(int boardId, int userId)
        {
            var board = await _context.Boards
                .Where(b => b.Id == boardId && (b.OwnerId == userId || b.Members.Any(m => m.UserId == userId)))
                .Include(b => b.Owner)
                .Include(b => b.Tags)
                .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
                .ThenInclude(c => c.CardTags)
                .ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync();

            if (board == null)
            {
                return Result<BoardDetailsDto>.NotFound($"Board with ID {boardId} not found or access denied");
            }

            var boardDetailsDto = board.ToDetailsDto();
            return Result<BoardDetailsDto>.Success(boardDetailsDto);
        }

        public async Task<Result<List<Board>>> GetOwnedBoardsAsync(int userId)
        {
            var boards = await _context.Boards
                .Where(b => b.OwnerId == userId)
                .Include(b => b.Owner)
                .OrderBy(b => b.UpdatedAt)
                .ToListAsync();

            return Result<List<Board>>.Success(boards);
        }

        public async Task<Result<List<Board>>> GetUserMembershipBoardsAsync(int userId)
        {
            var boards = await _context.Boards
                .Where(b => b.Members.Any(m => m.UserId == userId))
                .Include(b => b.Owner)
                .OrderBy(b => b.UpdatedAt)
                .ToListAsync();

            return Result<List<Board>>.Success(boards);
        }

        public async Task<Result<int>> CreateBoardAsync(CreateBoardDto dto, int ownerId)
        {
            var now = DateTime.UtcNow;
            var board = new Board
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                OwnerId = ownerId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            return Result<int>.Success(board.Id);
        }

        public async Task<Result<Board>> UpdateBoardAsync(int boardId, UpdateBoardDto dto, int userId)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == boardId && b.OwnerId == userId);

            if (board == null)
            {
                return Result<Board>.NotFound($"Board with ID {boardId} not found or access denied");
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                board.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                board.Description = dto.Description;
            }

            board.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<Board>.Success(board);
        }

        public async Task<Result<bool>> DeleteBoardAsync(int boardId, int userId)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == boardId && b.OwnerId == userId);

            if (board == null)
            {
                return Result<bool>.NotFound($"Board with ID {boardId} not found or access denied");
            }

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
