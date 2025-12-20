using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;
using StressTracker5001Server.Extensions;
using StressTracker5001Server.DTOs.ActivityLog;
using StressTracker5001Server.DTOs.Common;

namespace StressTracker5001Server.Services
{
    public interface IBoardService
    {
        Task<Result<Board>> GetBoardByIdAsync(int boardId, int userId);
        Task<Result<BoardDetailsDto>> GetBoardWithColumnsAndCardsAsync(int boardId, int userId);
        Task<Result<PagedResultDto<ActivityLogDto>>> GetActivityLogsForBoardAsync(
            int boardId, int userId, int page = 1, int pageSize = 10, int? entityType = null, int? actionType = null);
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
        private readonly IActivityLogService _activityLogService;

        public BoardService(AppDbContext context, IBoardAuthorizationService boardAuthorizationService, IActivityLogService activityLogService)
        {
            _context = context;
            _boardAuthorizationService = boardAuthorizationService;
            _activityLogService = activityLogService;
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
                .ThenInclude(c => c.CardAssignments)
                .ThenInclude(ca => ca.User)
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

        public async Task<Result<PagedResultDto<ActivityLogDto>>> GetActivityLogsForBoardAsync(
            int boardId, int userId, int page = 1, int pageSize = 10, int? entityType = null, int? actionType = null)
        {
            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Admin))
            {
                return Result<PagedResultDto<ActivityLogDto>>.Forbidden("You do not have permission to access this board's activity logs");
            }

            IQueryable<ActivityLog> query = _context.ActivityLogs
                .Where(al => al.BoardId == boardId)
                .Include(al => al.User);

            // Apply entity type filter
            if (entityType.HasValue)
            {
                query = query.Where(al => al.EntityType == (ActivityLogEntityType)entityType.Value);
            }

            // Apply action type filter
            if (actionType.HasValue)
            {
                query = query.Where(al => al.Action == (ActivityLogActionType)actionType.Value);
            }

            var totalCount = await query.CountAsync();
            var skip = (page - 1) * pageSize;

            var logs = await query
                .OrderByDescending(al => al.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var dtos = logs.Select(log => new ActivityLogDto
            {
                Id = log.Id,
                BoardId = log.BoardId,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                ActionType = log.Action,
                Description = log.Details,
                CreatedBy = new DTOs.User.UserDto
                {
                    Id = log.User!.Id,
                    Username = log.User.Username,
                    CreatedAt = log.User.CreatedAt,
                    UpdatedAt = log.User.UpdatedAt,
                },
                CreatedAt = log.CreatedAt,
            }).ToList();

            var hasMore = skip + pageSize < totalCount;
            var nextPage = page + 1;
            var previousPage = page > 1 ? page - 1 : 0;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var result = new PagedResultDto<ActivityLogDto>
            {
                Items = dtos,
                HasMore = hasMore,
                Page = page,
                NextPage = nextPage,
                PreviousPage = previousPage,
                PageSize = totalPages,
            };

            return Result<PagedResultDto<ActivityLogDto>>.Success(result);
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

            await _activityLogService.LogBoardCreatedAsync(board.Id, userId, board.Name);

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

            var copyOfOldBoard = new
            {
                board.Name,
                board.Description
            };

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                board.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                board.Description = dto.Description;
            }

            board.UpdatedAt = DateTime.UtcNow;

            await _activityLogService.LogBoardUpdatedAsync(board.Id, userId, copyOfOldBoard, board);

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

            await _activityLogService.LogBoardDeletedAsync(board.Id, userId, board.Name);

            return Result<bool>.Success(true);
        }
    }
}
