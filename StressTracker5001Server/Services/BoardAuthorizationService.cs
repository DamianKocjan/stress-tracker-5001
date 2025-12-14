using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface IBoardAuthorizationService
    {
        Task<Result<BoardMember>> AddMemberAsync(int boardId, int userMemberId, int userId, BoardMemberRole role);
        Task<Result<bool>> RemoveMemberAsync(int boardId, int userMemberId, int userId);
        Task<Result<List<BoardMember>>> GetMembersAsync(int boardId, int userId);
        Task<Result<BoardMember>> GetMemberAsync(int boardId, int userId);
        Task<Result<BoardMember>> ChangeMemberRoleAsync(int boardId, int userId, int userMemberId, BoardMemberRole newRole);
        Task<Result<BoardMemberRole>> GetBoardUserRoleByIdAsync(int boardId, int userId);
        Task<Result<bool>> IsUserBoardMemberAsync(int boardId, int userId);
        Task<bool> UserCanAccessBoardAsync(int boardId, int userId, BoardMemberRole? requiredRole = BoardMemberRole.Viewer);
    }

    public class BoardAuthorizationService : IBoardAuthorizationService
    {
        private readonly AppDbContext _context;

        public BoardAuthorizationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<BoardMember>> AddMemberAsync(int boardId, int userMemberId, int userId, BoardMemberRole role)
        {
            var existingMemberResult = await GetMemberAsync(boardId, userMemberId);
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

            // Check if user is board owner or has Admin role
            var isOwner = board.OwnerId == userId;
            if (!isOwner && !await UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Admin))
            {
                return Result<BoardMember>.Forbidden("You do not have permission to add members to this board");
            }

            var now = DateTime.UtcNow;
            var boardMember = new BoardMember
            {
                BoardId = boardId,
                UserId = userMemberId,
                Role = role,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.BoardMembers.Add(boardMember);
            await _context.SaveChangesAsync();
            return Result<BoardMember>.Success(boardMember);
        }

        public async Task<Result<bool>> RemoveMemberAsync(int boardId, int userMemberId, int userId)
        {
            var memberResult = await GetMemberAsync(boardId, userMemberId);
            if (!memberResult.IsSuccess)
            {
                return Result<bool>.NotFound("Board member not found");
            }

            if (!await UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Admin))
            {
                return Result<bool>.Forbidden("You do not have permission to remove this member from the board");
            }

            var member = memberResult.Value!;
            _context.BoardMembers.Remove(member);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<List<BoardMember>>> GetMembersAsync(int boardId, int userId)
        {
            // Validate board exists
            var board = await _context.Boards.FindAsync(boardId);
            if (board == null)
            {
                return Result<List<BoardMember>>.NotFound($"Board with ID {boardId} not found");
            }

            if (!await UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Admin))
            {
                return Result<List<BoardMember>>.Forbidden("You do not have permission to view members of this board");
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
                .Include(bm => bm.Board)
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

            if (!await UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Admin))
            {
                return Result<BoardMember>.Forbidden("You do not have permission to change member roles on this board");
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

        public async Task<bool> UserCanAccessBoardAsync(int boardId, int userId, BoardMemberRole? requiredRole = BoardMemberRole.Viewer)
        {
            // Check if user is a board member
            var memberResult = await GetMemberAsync(boardId, userId);

            // If user is a member, check their role
            if (memberResult.IsSuccess)
            {
                return memberResult.Value!.Role >= requiredRole;
            }

            // If not a member, check if they're the board owner
            var board = await _context.Boards.FindAsync(boardId);
            if (board != null && board.OwnerId == userId)
            {
                return true;
            }

            return false;
        }
    }
}
