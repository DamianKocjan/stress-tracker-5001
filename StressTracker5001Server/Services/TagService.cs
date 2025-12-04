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
        Task<TagDto?> CreateTagAsync(TagCreateDto dto, int ownerId);
        Task<TagDto?> UpdateTagAsync(int tagId, TagUpdateDto dto, int ownerId);
        Task<bool> DeleteTagAsync(int tagId, int ownerId);
    }

    public class TagService : ITagService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _maxTagsPerBoard;

        public TagService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _maxTagsPerBoard = _configuration.GetValue("Tags:MaxTagsPerBoard", 20);
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

        public async Task<TagDto?> CreateTagAsync(TagCreateDto dto, int ownerId)
        {
            var board = await _context.Boards.FindAsync(dto.BoardId);
            if (board == null || board.OwnerId != ownerId)
            {
                return null;
            }

            // Check if board has reached the configured tag limit
            var tagCount = await _context.Tags.CountAsync(t => t.BoardId == dto.BoardId);
            if (tagCount >= _maxTagsPerBoard)
            {
                return null;
            }

            // Check if tag name already exists in this board (case-insensitive)
            // TODO: Improve performance of case-insensitive check for large datasets
            var nameExists = await _context.Tags
                .AnyAsync(t => t.BoardId == dto.BoardId && t.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
            {
                return null;
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
            // TODO: Improve performance of case-insensitive check for large datasets
            var nameExists = await _context.Tags
                .AnyAsync(t => t.BoardId == tag.BoardId && t.Id != tagId && t.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
            {
                return null;
            }

            tag.Name = dto.Name;
            tag.Color = dto.Color;
            tag.UpdatedAt = DateTime.UtcNow;

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
    }
}