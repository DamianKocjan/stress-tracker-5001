using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Services
{
    public interface ITagService
    {
        Task<List<TagDto>> GetTagsByBoardIdAsync(int boardId, int ownerId);
        Task<TagDto?> GetTagByIdAsync(int tagId, int ownerId);
        Task<TagDto> CreateTagAsync(TagCreateDto dto, int ownerId);
        Task<TagDto?> UpdateTagAsync(int tagId, TagUpdateDto dto, int ownerId);
        Task<bool> DeleteTagAsync(int tagId, int ownerId);
        Task<bool> AssignTagToCardAsync(int cardId, int tagId, int ownerId);
        Task<bool> RemoveTagFromCardAsync(int cardId, int tagId, int ownerId);
        Task<List<TagDto>> GetTagsByCardIdAsync(int cardId, int ownerId);
    }

    public class TagService : ITagService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _maxTagsPerBoard;
        private readonly int _maxTagsPerCard;

        public TagService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _maxTagsPerBoard = _configuration.GetValue("Tags:MaxTagsPerBoard", 20);
            _maxTagsPerCard = _configuration.GetValue("Tags:MaxTagsPerCard", 5);
        }

        public async Task<List<TagDto>> GetTagsByBoardIdAsync(int boardId, int ownerId)
        {
            var tags = await _context.Tags
                .Where(t => t.BoardId == boardId && t.Board!.OwnerId == ownerId)
                .OrderBy(t => t.Name)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Color = t.Color,
                    BoardId = t.BoardId,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return tags;
        }

        public async Task<TagDto?> GetTagByIdAsync(int tagId, int ownerId)
        {
            var tag = await _context.Tags
                .Where(t => t.Id == tagId && t.Board!.OwnerId == ownerId)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Color = t.Color,
                    BoardId = t.BoardId,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return tag;
        }

        public async Task<TagDto> CreateTagAsync(TagCreateDto dto, int ownerId)
        {
            var board = await _context.Boards.FindAsync(dto.BoardId);
            if (board == null || board.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Board not found or access denied");
            }

            // Check if board has reached the configured tag limit
            var tagCount = await _context.Tags.CountAsync(t => t.BoardId == dto.BoardId);
            if (tagCount >= _maxTagsPerBoard)
            {
                throw new InvalidOperationException($"Board has reached the maximum limit of {_maxTagsPerBoard} tags");
            }

            // Check if tag name already exists in this board (case-insensitive)
            var nameExists = await _context.Tags
                .AnyAsync(t => t.BoardId == dto.BoardId && t.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
            {
                throw new InvalidOperationException("A tag with this name already exists in this board");
            }

            var tag = new Tag
            {
                Name = dto.Name,
                Color = dto.Color,
                BoardId = dto.BoardId
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color,
                BoardId = tag.BoardId,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt
            };
        }

        public async Task<TagDto?> UpdateTagAsync(int tagId, TagUpdateDto dto, int ownerId)
        {
            var tag = await _context.Tags.Include(t => t.Board).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null || tag.Board?.OwnerId != ownerId)
            {
                return null;
            }

            // Check if new name already exists in this board (excluding current tag)
            var nameExists = await _context.Tags
                .AnyAsync(t => t.BoardId == tag.BoardId && t.Id != tagId && t.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
            {
                throw new InvalidOperationException("A tag with this name already exists in this board");
            }

            tag.Name = dto.Name;
            tag.Color = dto.Color;

            await _context.SaveChangesAsync();

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color,
                BoardId = tag.BoardId,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt
            };
        }

        public async Task<bool> DeleteTagAsync(int tagId, int ownerId)
        {
            var tag = await _context.Tags.Include(t => t.Board).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null || tag.Board?.OwnerId != ownerId)
            {
                return false;
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssignTagToCardAsync(int cardId, int tagId, int ownerId)
        {
            var card = await _context.Cards
                .Include(c => c.CardTags)
                .Include(c => c.Column)
                .ThenInclude(col => col!.Board)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null || card.Column?.Board?.OwnerId != ownerId)
            {
                return false;
            }

            var tag = await _context.Tags.Include(t => t.Board).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null || tag.Board?.OwnerId != ownerId)
            {
                return false;
            }

            // Check if card already has the maximum number of tags
            if (card.CardTags.Count >= _maxTagsPerCard)
            {
                throw new InvalidOperationException($"Card has reached the maximum limit of {_maxTagsPerCard} tags");
            }

            // Check if tag is already assigned to card
            if (card.CardTags.Any(ct => ct.TagId == tagId))
            {
                throw new InvalidOperationException("Tag is already assigned to this card");
            }

            var cardTag = new CardTag
            {
                CardId = cardId,
                TagId = tagId
            };

            _context.CardTags.Add(cardTag);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveTagFromCardAsync(int cardId, int tagId, int ownerId)
        {
            var cardTag = await _context.CardTags
                .Include(ct => ct.Card)
                .ThenInclude(c => c!.Column)
                .ThenInclude(col => col!.Board)
                .FirstOrDefaultAsync(ct => ct.CardId == cardId && ct.TagId == tagId);

            if (cardTag == null || cardTag.Card?.Column?.Board?.OwnerId != ownerId)
            {
                return false;
            }

            _context.CardTags.Remove(cardTag);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<TagDto>> GetTagsByCardIdAsync(int cardId, int ownerId)
        {
            var tags = await _context.CardTags
                .Where(ct => ct.CardId == cardId && ct.Card!.Column!.Board!.OwnerId == ownerId)
                .Include(ct => ct.Tag)
                .Select(ct => new TagDto
                {
                    Id = ct.Tag!.Id,
                    Name = ct.Tag.Name,
                    Color = ct.Tag.Color,
                    BoardId = ct.Tag.BoardId,
                    CreatedAt = ct.Tag.CreatedAt,
                    UpdatedAt = ct.Tag.UpdatedAt
                })
                .ToListAsync();

            return tags;
        }
    }
}