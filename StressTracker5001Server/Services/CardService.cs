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
        Task<Card> CreateCardAsync(CreateCardDto dto, int ownerId);
        Task<Card?> UpdateCardAsync(int cardId, UpdateCardDto dto, int ownerId);
        Task<bool> MoveCardAsync(int cardId, int newPosition, int ownerId);
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
                .FirstOrDefaultAsync(c => c.Id == cardId && c.Column.Board.OwnerId == ownerId);
        }

        public async Task<Card?> GetCardDetailsByIdAsync(int cardId, int ownerId)
        {
            return await _context.Cards
                .Include(c => c.Column)
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == cardId && c.Column.Board.OwnerId == ownerId);
        }

        public async Task<List<Card>> GetCardsByColumnIdAsync(int columnId, int ownerId)
        {
            return await _context.Cards
                .Where(c => c.ColumnId == columnId && c.Column.Board.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<Card> CreateCardAsync(CreateCardDto dto, int ownerId)
        {
            var card = new Card
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                ColumnId = dto.ColumnId,
                Position = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<Card?> UpdateCardAsync(int cardId, UpdateCardDto dto, int ownerId)
        {
            var card = await GetCardByIdAsync(cardId, ownerId);
            if (card == null) return null;

            card.Title = dto.Title;
            card.Description = dto.Description;
            card.DueDate = dto.DueDate;
            card.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<bool> MoveCardAsync(int cardId, int newPosition, int ownerId)
        {
            var card = await GetCardByIdAsync(cardId, ownerId);
            if (card == null) return false;

            card.Position = newPosition;
            card.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCardAsync(int cardId, int ownerId)
        {
            var card = await GetCardByIdAsync(cardId, ownerId);
            if (card == null) return false;

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
