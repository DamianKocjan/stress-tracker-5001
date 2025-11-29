using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Services
{
    public interface ICardService
    {
        Task<Card?> GetCardByIdAsync(int cardId, int ownerId);
        Task<Card?> GetCardDetailsByIdAsync(int cardId, int ownerId);
        Task<List<Card>> GetCardsByColumnIdAsync(int columnId, int ownerId);
        Task<Card> CreateCardAsync(int columnId, CreateCardDto dto, int userId);
        Task<Card?> UpdateCardAsync(int cardId, UpdateCardDto dto, int ownerId);
        Task<bool> MoveCardAsync(int cardId, MoveCardDto dto, int ownerId);
        Task<bool> DeleteCardAsync(int cardId, int ownerId);
    }

    public class CardService : ICardService
    {
        private readonly AppDbContext _context;

        public CardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Card?> GetCardByIdAsync(int cardId, int ownerId)
        {
            return await _context.Cards
                .Include(c => c.Column)
                .ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == cardId && c.Column.Board.OwnerId == ownerId);
        }

        public async Task<Card?> GetCardDetailsByIdAsync(int cardId, int ownerId)
        {
            return await _context.Cards
                .Include(c => c.Column)
                .ThenInclude(c => c.Board)
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == cardId && c.Column.Board.OwnerId == ownerId);
        }

        public async Task<List<Card>> GetCardsByColumnIdAsync(int columnId, int ownerId)
        {
            return await _context.Cards
                .Include(c => c.Column)
                .ThenInclude(c => c.Board)
                .Where(c => c.ColumnId == columnId && c.Column.Board.OwnerId == ownerId)
                .OrderBy(c => c.Position)
                .ToListAsync();
        }

        public async Task<Card> CreateCardAsync(int columnId, CreateCardDto dto, int userId)
        {
            var cardCount = _context.Cards
                .Include(c => c.Column)
                .ThenInclude(c => c.Board)
                .Where(c => c.ColumnId == columnId && c.Column.Board.OwnerId == userId)
                .Count();

            var card = new Card
            {
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                DueDate = dto.DueDate,
                ColumnId = columnId,
                CreatedById = userId,
                Position = cardCount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Cards.Add(card);

            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<Card?> UpdateCardAsync(int cardId, UpdateCardDto dto, int ownerId)
        {
            var card = await GetCardByIdAsync(cardId, ownerId);
            if (card == null)
            {
                return null;
            }

            card.Title = dto.Title;
            card.Description = dto.Description ?? string.Empty;
            card.DueDate = dto.DueDate;
            card.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<bool> MoveCardAsync(int cardId, MoveCardDto dto, int ownerId)
        {
            var card = await GetCardByIdAsync(cardId, ownerId);
            if (card == null)
            {
                return false;
            }

            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == dto.NewColumnId && c.Board.OwnerId == ownerId);

            if (column == null)
            {
                return false;
            }

            if (column.WipLimit != null)
            {
                var cardCountInTargetColumn = await _context.Cards
                    .Where(c => c.ColumnId == dto.NewColumnId)
                    .CountAsync();

                if (cardCountInTargetColumn >= column.WipLimit)
                {
                    return false; // Exceeds WIP limit
                }
            }

            card.Position = dto.NewPosition;
            card.ColumnId = dto.NewColumnId;
            card.UpdatedAt = DateTime.UtcNow;

            // Update the positions of other cards in the same column
            var cards = await _context.Cards
                .Where(c => c.ColumnId == dto.NewColumnId && c.Id != cardId)
                .OrderBy(c => c.Position)
                .ToListAsync();

            for (int i = 0; i < cards.Count; i++)
            {
                if (i >= dto.NewPosition)
                {
                    cards[i].Position = i + 1;
                }
                else
                {
                    cards[i].Position = i;
                }
                cards[i].UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCardAsync(int cardId, int ownerId)
        {
            var card = await GetCardByIdAsync(cardId, ownerId);
            if (card == null)
            {
                return false;
            }

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
