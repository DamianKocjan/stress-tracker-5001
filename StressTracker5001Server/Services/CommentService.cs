using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface ICommentService
    {
        Task<Result<Comment>> GetCommentByIdAsync(int commentId, int userId);
        Task<Result<Comment>> UpdateCommentAsync(int commentId, UpdateCommentDto dto, int userId);
        Task<Result<bool>> DeleteCommentAsync(int commentId, int userId);
    }

    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Comment>> GetCommentByIdAsync(int commentId, int userId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

            if (comment == null)
            {
                return Result<Comment>.NotFound($"Comment with ID {commentId} not found or access denied");
            }

            return Result<Comment>.Success(comment);
        }

        public async Task<Result<Comment>> UpdateCommentAsync(int commentId, UpdateCommentDto dto, int userId)
        {
            var commentResult = await GetCommentByIdAsync(commentId, userId);
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
            var commentResult = await GetCommentByIdAsync(commentId, userId);
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
