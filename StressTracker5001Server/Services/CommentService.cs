using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface ICommentService
    {
        Task<Result<Comment>> GetCommentByIdAsync(int commentId, int userId, BoardMemberRole requiredRole = BoardMemberRole.Viewer);
        Task<Result<Comment>> UpdateCommentAsync(int commentId, UpdateCommentDto dto, int userId);
        Task<Result<bool>> DeleteCommentAsync(int commentId, int userId);
    }

    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly IBoardAuthorizationService _boardAuthorizationService;
        private readonly IActivityLogService _activityLogService;

        public CommentService(AppDbContext context, IBoardAuthorizationService boardAuthorizationService, IActivityLogService activityLogService)
        {
            _context = context;
            _boardAuthorizationService = boardAuthorizationService;
            _activityLogService = activityLogService;
        }

        public async Task<Result<Comment>> GetCommentByIdAsync(int commentId, int userId, BoardMemberRole requiredRole = BoardMemberRole.Viewer)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Card)
                .ThenInclude(card => card!.Column)
                .ThenInclude(column => column!.Board)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return Result<Comment>.NotFound($"Comment with ID {commentId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(comment.Card!.Column!.BoardId, userId, requiredRole))
            {
                return Result<Comment>.Forbidden("You do not have permission to access this comment");
            }

            return Result<Comment>.Success(comment);
        }

        public async Task<Result<Comment>> UpdateCommentAsync(int commentId, UpdateCommentDto dto, int userId)
        {
            var commentResult = await GetCommentByIdAsync(commentId, userId, BoardMemberRole.Member);
            if (!commentResult.IsSuccess)
            {
                return commentResult;
            }

            // Check if user is the author or an admin/owner of the board
            var userRole = await _boardAuthorizationService.GetMemberAsync(commentResult.Value!.Card!.Column!.BoardId, userId);
            if (commentResult.Value.UserId != userId && (userRole == null || (userRole.Value!.Role != BoardMemberRole.Admin && userRole.Value.Role != BoardMemberRole.Owner)))
            {
                return Result<Comment>.Forbidden("You do not have permission to edit this comment");
            }

            var comment = commentResult.Value!;
            var oldContent = comment.Content;
            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLogService.LogCommentUpdatedAsync(comment.Card!.Column!.BoardId, userId, commentId, comment.CardId, oldContent, comment.Content);

            return Result<Comment>.Success(comment);
        }

        public async Task<Result<bool>> DeleteCommentAsync(int commentId, int userId)
        {
            var commentResult = await GetCommentByIdAsync(commentId, userId, BoardMemberRole.Member);
            if (!commentResult.IsSuccess)
            {
                return Result<bool>.NotFound(commentResult.Error ?? "Comment not found");
            }

            // Check if user is the author or an admin/owner of the board
            var userRole = await _boardAuthorizationService.GetMemberAsync(commentResult.Value!.Card!.Column!.BoardId, userId);
            if (commentResult.Value.UserId != userId && (userRole == null || (userRole.Value!.Role != BoardMemberRole.Admin && userRole.Value.Role != BoardMemberRole.Owner)))
            {
                return Result<bool>.Forbidden("You do not have permission to delete this comment");
            }

            var comment = commentResult.Value!;
            var content = comment.Content;
            var boardId = comment.Card!.Column!.BoardId;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            await _activityLogService.LogCommentDeletedAsync(boardId, userId, commentId, comment.CardId, content);

            return Result<bool>.Success(true);
        }
    }
}
