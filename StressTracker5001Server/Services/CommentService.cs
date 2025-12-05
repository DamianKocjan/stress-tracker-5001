using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Services
{
    public interface ICommentService
    {
        Task<Comment?> GetCommentByIdAsync(int commentId, int userId);
        Task<Comment?> UpdateCommentAsync(int commentId, UpdateCommentDto dto, int userId);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
    }

    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId, int userId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
        }

        public async Task<Comment?> UpdateCommentAsync(int commentId, UpdateCommentDto dto, int userId)
        {
            var comment = await GetCommentByIdAsync(commentId, userId);
            if (comment == null)
            {
                return null;
            }

            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await GetCommentByIdAsync(commentId, userId);
            if (comment == null)
            {
                return false;
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
