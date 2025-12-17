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
        Task<Result<Board>> CreateBoardAsync(CreateBoardDto dto, int userId);
        Task<Result<Board>> UpdateBoardAsync(int boardId, UpdateBoardDto dto, int userId);
        Task<Result<bool>> DeleteBoardAsync(int boardId, int userId);
    }

    public class BoardService : IBoardService
    {
        private readonly AppDbContext _context;
        private readonly IBoardAuthorizationService _boardAuthorizationService;

        public BoardService(AppDbContext context, IBoardAuthorizationService boardAuthorizationService)
        {
            _context = context;
            _boardAuthorizationService = boardAuthorizationService;
        }

        public async Task<Result<Board>> GetBoardByIdAsync(int boardId, int userId)
        {
            var board = await _context.Boards
                .Include(b => b.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(b => b.Id == boardId);

            if (board == null)
            {
                return Result<Board>.NotFound($"Board with ID {boardId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(boardId, userId))
            {
                return Result<Board>.Forbidden("You do not have permission to access this board");
            }

            return Result<Board>.Success(board);
        }

        public async Task<Result<BoardDetailsDto>> GetBoardWithColumnsAndCardsAsync(int boardId, int userId)
        {
            var board = await _context.Boards
                .Where(b => b.Id == boardId && b.Members.Any(m => m.UserId == userId))
                .Include(b => b.Members)
                .ThenInclude(m => m.User)
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

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(boardId, userId))
            {
                return Result<BoardDetailsDto>.Forbidden("You do not have permission to access this board");
            }

            var boardDetailsDto = board.ToDetailsDto();
            return Result<BoardDetailsDto>.Success(boardDetailsDto);
        }

        public async Task<Result<List<Board>>> GetOwnedBoardsAsync(int userId)
        {
            var boards = await _context.Boards
                .Where(b => b.Members.Any(m => m.UserId == userId && m.Role == BoardMemberRole.Owner))
                .Include(b => b.Members)
                .ThenInclude(m => m.User)
                .OrderBy(b => b.UpdatedAt)
                .ToListAsync();

            return Result<List<Board>>.Success(boards);
        }

        public async Task<Result<List<Board>>> GetUserMembershipBoardsAsync(int userId)
        {
            var boards = await _context.Boards
                .Include(b => b.Members)
                .Where(b => b.Members.Any(m => m.UserId == userId && m.Role != BoardMemberRole.Owner))
                .OrderBy(b => b.UpdatedAt)
                .ToListAsync();

            return Result<List<Board>>.Success(boards);
        }

        public async Task<Result<Board>> CreateBoardAsync(CreateBoardDto dto, int userId)
        {
            var now = DateTime.UtcNow;
            var board = new Board
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                CreatedAt = now,
                UpdatedAt = now
            };

            // Create the owner as a BoardMember with Owner role
            var ownerMember = new BoardMember
            {
                UserId = userId,
                Role = BoardMemberRole.Owner,
                CreatedAt = now,
                UpdatedAt = now
            };

            board.Members.Add(ownerMember);
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            return Result<Board>.Success(board);
        }

        public async Task<Result<Board>> UpdateBoardAsync(int boardId, UpdateBoardDto dto, int userId)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == boardId);

            if (board == null)
            {
                return Result<Board>.NotFound($"Board with ID {boardId} not found or access denied");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Admin))
            {
                return Result<Board>.Forbidden("You do not have permission to update this board");
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
                .Include(b => b.Members)
                .FirstOrDefaultAsync(b => b.Id == boardId);

            if (board == null)
            {
                return Result<bool>.NotFound($"Board with ID {boardId} not found");
            }

            // Check if user is owner
            var isOwner = board.Members.Any(m => m.UserId == userId && m.Role == BoardMemberRole.Owner);
            if (!isOwner)
            {
                return Result<bool>.Forbidden("Only the board owner can delete the board");
            }

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
