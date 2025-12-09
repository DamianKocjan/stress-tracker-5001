using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface ITagService
    {
        Task<Result<List<Tag>>> GetTagsByBoardIdAsync(int boardId, int ownerId);
        Task<Result<Tag>> GetTagByIdAsync(int tagId, int ownerId);
        Task<Result<Tag>> CreateTagAsync(TagCreateDto dto, int ownerId);
        Task<Result<Tag>> UpdateTagAsync(int tagId, TagUpdateDto dto, int ownerId);
        Task<Result<bool>> DeleteTagAsync(int tagId, int ownerId);
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

        public async Task<Result<List<Tag>>> GetTagsByBoardIdAsync(int boardId, int ownerId)
        {
            // Validate board exists and user has access
            var board = await _context.Boards.FindAsync(boardId);
            if (board == null || board.OwnerId != ownerId)
            {
                return Result<List<Tag>>.NotFound($"Board with ID {boardId} not found or access denied");
            }

            var tags = await _context.Tags
                .Where(t => t.BoardId == boardId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return Result<List<Tag>>.Success(tags);
        }

        public async Task<Result<Tag>> GetTagByIdAsync(int tagId, int ownerId)
        {
            var tag = await _context.Tags
                .Include(t => t.Board)
                .FirstOrDefaultAsync(t => t.Id == tagId && t.Board!.OwnerId == ownerId);

            if (tag == null)
            {
                return Result<Tag>.NotFound($"Tag with ID {tagId} not found or access denied");
            }

            return Result<Tag>.Success(tag);
        }

        public async Task<Result<Tag>> CreateTagAsync(TagCreateDto dto, int ownerId)
        {
            var board = await _context.Boards.FindAsync(dto.BoardId);
            if (board == null || board.OwnerId != ownerId)
            {
                return Result<Tag>.NotFound($"Board with ID {dto.BoardId} not found or access denied");
            }

            // Check if board has reached the configured tag limit
            var tagCount = await _context.Tags.CountAsync(t => t.BoardId == dto.BoardId);
            if (tagCount >= _maxTagsPerBoard)
            {
                return Result<Tag>.Failure($"Board has reached maximum tag limit of {_maxTagsPerBoard}", 400);
            }

            // Check if tag name already exists in this board (case-insensitive)
            var nameExists = await _context.Tags
                .AnyAsync(t => t.BoardId == dto.BoardId && t.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
            {
                return Result<Tag>.Failure($"Tag with name '{dto.Name}' already exists in this board", 400);
            }

            var now = DateTime.UtcNow;
            var tag = new Tag
            {
                Name = dto.Name,
                Color = dto.Color,
                BoardId = dto.BoardId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return Result<Tag>.Success(tag);
        }

        public async Task<Result<Tag>> UpdateTagAsync(int tagId, TagUpdateDto dto, int ownerId)
        {
            var tag = await _context.Tags.Include(t => t.Board).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null || tag.Board?.OwnerId != ownerId)
            {
                return Result<Tag>.NotFound($"Tag with ID {tagId} not found or access denied");
            }

            // Check if new name already exists in this board (excluding current tag)
            var nameExists = await _context.Tags
                .AnyAsync(t => t.BoardId == tag.BoardId && t.Id != tagId && t.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
            {
                return Result<Tag>.Failure($"Tag with name '{dto.Name}' already exists in this board", 400);
            }

            tag.Name = dto.Name;
            tag.Color = dto.Color;
            tag.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<Tag>.Success(tag);
        }

        public async Task<Result<bool>> DeleteTagAsync(int tagId, int ownerId)
        {
            var tag = await _context.Tags.Include(t => t.Board).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null || tag.Board?.OwnerId != ownerId)
            {
                return Result<bool>.NotFound($"Tag with ID {tagId} not found or access denied");
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}