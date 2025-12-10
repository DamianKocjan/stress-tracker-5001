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

        public CommentService(AppDbContext context, IBoardAuthorizationService boardAuthorizationService)
        {
            _context = context;
            _boardAuthorizationService = boardAuthorizationService;
        }

        public async Task<Result<Comment>> GetCommentByIdAsync(int commentId, int userId, BoardMemberRole requiredRole = BoardMemberRole.Viewer)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Card)
                .ThenInclude(card => card.Column)
                .ThenInclude(column => column.Board)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return Result<Comment>.NotFound($"Comment with ID {commentId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(comment.Card.Column.BoardId, userId, requiredRole))
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

            var comment = commentResult.Value!;
            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<Comment>.Success(comment);
        }

        public async Task<Result<bool>> DeleteCommentAsync(int commentId, int userId)
        {
            var commentResult = await GetCommentByIdAsync(commentId, userId, BoardMemberRole.Member);
            if (!commentResult.IsSuccess)
            {
                return Result<bool>.NotFound(commentResult.Error ?? "Comment not found");
            }

            var comment = commentResult.Value!;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
