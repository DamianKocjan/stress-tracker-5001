using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface IBoardAuthorizationService
    {
        Task<Result<BoardMember>> AddMemberAsync(int boardId, int userId, BoardMemberRole role);
        Task<Result<bool>> RemoveMemberAsync(int boardId, int userId);
        Task<Result<List<BoardMember>>> GetMembersAsync(int boardId);
        Task<Result<BoardMember>> GetMemberAsync(int boardId, int userId);
        Task<Result<BoardMember>> ChangeMemberRoleAsync(int boardId, int userId, int userMemberId, BoardMemberRole newRole);
        Task<Result<BoardMemberRole>> GetBoardUserRoleByIdAsync(int boardId, int userId);
        Task<Result<bool>> IsUserBoardMemberAsync(int boardId, int userId);
    }

    public class BoardAuthorizationService : IBoardAuthorizationService
    {
        private readonly AppDbContext _context;

        public BoardAuthorizationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<BoardMember>> AddMemberAsync(int boardId, int userId, BoardMemberRole role)
        {
            var existingMemberResult = await GetMemberAsync(boardId, userId);
            if (existingMemberResult.IsSuccess)
            {
                return Result<BoardMember>.Failure("User is already a member of this board", 400);
            }

            // Validate board exists
            var board = await _context.Boards.FindAsync(boardId);
            if (board == null)
            {
                return Result<BoardMember>.NotFound($"Board with ID {boardId} not found");
            }

            var now = DateTime.UtcNow;
            var boardMember = new BoardMember
            {
                BoardId = boardId,
                UserId = userId,
                Role = role,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.BoardMembers.Add(boardMember);
            await _context.SaveChangesAsync();
            return Result<BoardMember>.Success(boardMember);
        }

        public async Task<Result<bool>> RemoveMemberAsync(int boardId, int userId)
        {
            var memberResult = await GetMemberAsync(boardId, userId);
            if (!memberResult.IsSuccess)
            {
                return Result<bool>.NotFound("Board member not found");
            }

            var member = memberResult.Value!;
            _context.BoardMembers.Remove(member);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<List<BoardMember>>> GetMembersAsync(int boardId)
        {
            // Validate board exists
            var board = await _context.Boards.FindAsync(boardId);
            if (board == null)
            {
                return Result<List<BoardMember>>.NotFound($"Board with ID {boardId} not found");
            }

            var members = await _context.BoardMembers
                .Include(bm => bm.User)
                .Where(bm => bm.BoardId == boardId)
                .ToListAsync();

            return Result<List<BoardMember>>.Success(members);
        }

        public async Task<Result<BoardMember>> GetMemberAsync(int boardId, int userId)
        {
            var member = await _context.BoardMembers
                .Include(bm => bm.User)
                .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId);

            if (member == null)
            {
                return Result<BoardMember>.NotFound($"Board member not found");
            }

            return Result<BoardMember>.Success(member);
        }

        public async Task<Result<BoardMember>> ChangeMemberRoleAsync(int boardId, int userId, int userMemberId, BoardMemberRole newRole)
        {
            var memberResult = await GetMemberAsync(boardId, userMemberId);
            if (!memberResult.IsSuccess)
            {
                return Result<BoardMember>.NotFound("Board member not found");
            }

            var member = memberResult.Value!;
            member.Role = newRole;
            member.UpdatedAt = DateTime.UtcNow;
            _context.BoardMembers.Update(member);
            await _context.SaveChangesAsync();
            return Result<BoardMember>.Success(member);
        }

        public async Task<Result<BoardMemberRole>> GetBoardUserRoleByIdAsync(int boardId, int userId)
        {
            var memberResult = await GetMemberAsync(boardId, userId);
            if (!memberResult.IsSuccess)
            {
                return Result<BoardMemberRole>.NotFound("User is not a member of this board");
            }

            return Result<BoardMemberRole>.Success(memberResult.Value!.Role);
        }

        public async Task<Result<bool>> IsUserBoardMemberAsync(int boardId, int userId)
        {
            var memberResult = await GetMemberAsync(boardId, userId);
            return Result<bool>.Success(memberResult.IsSuccess);
        }
    }
}
