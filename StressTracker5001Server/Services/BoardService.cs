using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Services
{
    public interface IBoardService
    {
        Task<Board?> GetBoardByIdAsync(int boardId, int ownerId);
        Task<BoardDetailsDto?> GetBoardWithColumnsAndCardsAsync(int boardId, int userId);
        Task<List<Board>> GetBoardsByOwnerIdAsync(int ownerId);
        Task<int> CreateBoardAsync(CreateBoardDto dto, int ownerId);
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

        public async Task<BoardDetailsDto?> GetBoardWithColumnsAndCardsAsync(int boardId, int userId)
        {
            var board = await _context.Boards
                .Where(b => b.Id == boardId && b.OwnerId == userId)
                .Include(b => b.Owner)
                .Include(b => b.Tags)
                .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
                .ThenInclude(c => c.CardTags)
                .ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync();

            if (board == null)
            {
                return null;
            }

            var boardDetailsDto = new BoardDetailsDto
            {
                Id = board.Id,
                Name = board.Name,
                Description = board.Description,
                OwnerId = board.OwnerId,
                Owner = new DTOs.User.UserDto
                {
                    Id = board.Owner.Id,
                    Email = board.Owner.Email,
                    Username = board.Owner.Username,
                    CreatedAt = board.Owner.CreatedAt,
                    UpdatedAt = board.Owner.UpdatedAt,
                },
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt,
                Columns = board.Columns.Select(c => new ColumnDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    BoardId = c.BoardId,
                    Position = c.Position,
                    WipLimit = c.WipLimit,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).OrderBy(c => c.Position).ToList(),
                Cards = board.Columns.SelectMany(c => c.Cards).Select(card => new CardDto
                {
                    Id = card.Id,
                    Title = card.Title,
                    Description = card.Description,
                    ColumnId = card.ColumnId,
                    CreatedById = card.CreatedById,
                    Position = card.Position,
                    DueDate = card.DueDate,
                    CreatedAt = card.CreatedAt,
                    UpdatedAt = card.UpdatedAt,
                    Tags = card.CardTags.Select(ct => ct.TagId).ToList()
                }).ToList(),
                Tags = board.Tags.Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Color = t.Color,
                    BoardId = t.BoardId,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList()
            };

            return boardDetailsDto;
        }

        public async Task<List<Board>> GetBoardsByOwnerIdAsync(int ownerId)
        {
            return await _context.Boards
                .Where(b => b.OwnerId == ownerId)
                .Include(b => b.Owner)
                .OrderBy(b => b.UpdatedAt)
                .ToListAsync();
        }

        public async Task<int> CreateBoardAsync(CreateBoardDto dto, int ownerId)
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

            return board.Id;
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
            board.UpdatedAt = DateTime.UtcNow;

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
