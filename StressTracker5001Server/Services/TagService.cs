using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface ITagService
    {
        Task<Result<List<Tag>>> GetTagsByBoardIdAsync(int boardId, int userId);
        Task<Result<Tag>> CreateTagAsync(TagCreateDto dto, int userId);
        Task<Result<Tag>> UpdateTagAsync(int tagId, TagUpdateDto dto, int userId);
        Task<Result<bool>> DeleteTagAsync(int tagId, int userId);
    }

    public class TagService : ITagService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IBoardAuthorizationService _boardAuthorizationService;
        private readonly IActivityLogService _activityLogService;

        private readonly int _maxTagsPerBoard;

        public TagService(AppDbContext context, IConfiguration configuration, IBoardAuthorizationService boardAuthorizationService, IActivityLogService activityLogService)
        {
            _context = context;
            _configuration = configuration;
            _boardAuthorizationService = boardAuthorizationService;
            _activityLogService = activityLogService;

            _maxTagsPerBoard = _configuration.GetValue("Tags:MaxTagsPerBoard", 20);
        }

        public async Task<Result<List<Tag>>> GetTagsByBoardIdAsync(int boardId, int userId)
        {
            // Validate board exists and user has access
            var board = await _context.Boards.FindAsync(boardId);
            if (board == null)
            {
                return Result<List<Tag>>.NotFound($"Board with ID {boardId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Viewer))
            {
                return Result<List<Tag>>.Forbidden("You do not have permission to access tags for this board");
            }

            var tags = await _context.Tags
                .Where(t => t.BoardId == boardId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return Result<List<Tag>>.Success(tags);
        }

        public async Task<Result<Tag>> CreateTagAsync(TagCreateDto dto, int userId)
        {
            var board = await _context.Boards.FindAsync(dto.BoardId);
            if (board == null)
            {
                return Result<Tag>.NotFound($"Board with ID {dto.BoardId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(dto.BoardId, userId, BoardMemberRole.Admin))
            {
                return Result<Tag>.Forbidden("You do not have permission to create tags for this board");
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

            await _activityLogService.LogTagCreatedAsync(dto.BoardId, userId, tag.Id, tag.Name);

            return Result<Tag>.Success(tag);
        }

        public async Task<Result<Tag>> UpdateTagAsync(int tagId, TagUpdateDto dto, int userId)
        {
            var tag = await _context.Tags.Include(t => t.Board).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null)
            {
                return Result<Tag>.NotFound($"Tag with ID {tagId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(tag.BoardId, userId, BoardMemberRole.Admin))
            {
                return Result<Tag>.Forbidden("You do not have permission to update this tag");
            }

            // Check if new name already exists in this board (excluding current tag)
            var nameExists = await _context.Tags
                .AnyAsync(t => t.BoardId == tag.BoardId && t.Id != tagId && t.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
            {
                return Result<Tag>.Failure($"Tag with name '{dto.Name}' already exists in this board", 400);
            }

            var oldTag = new { tag.Name, tag.Color };

            tag.Name = dto.Name;
            tag.Color = dto.Color;
            tag.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLogService.LogTagUpdatedAsync(tag.BoardId, userId, tagId, oldTag, new { Name = tag.Name, Color = tag.Color });

            return Result<Tag>.Success(tag);
        }

        public async Task<Result<bool>> DeleteTagAsync(int tagId, int userId)
        {
            var tag = await _context.Tags.Include(t => t.Board).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null)
            {
                return Result<bool>.NotFound($"Tag with ID {tagId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(tag.BoardId, userId, BoardMemberRole.Admin))
            {
                return Result<bool>.Forbidden("You do not have permission to delete this tag");
            }

            var tagName = tag.Name;
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            await _activityLogService.LogTagDeletedAsync(tag.BoardId, userId, tagId, tagName);

            return Result<bool>.Success(true);
        }
    }
}