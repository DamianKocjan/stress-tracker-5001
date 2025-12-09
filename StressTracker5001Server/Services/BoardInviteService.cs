using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface IBoardInviteService
    {
        Task<Result<BoardInvite>> GenerateInviteAsync(int boardId, int userId, BoardMemberRole role = BoardMemberRole.Member);
        Task<Result<bool>> CanGenerateInviteAsync(int boardId, int userId);
        Task<Result<bool>> ValidateInviteCodeAsync(string code, int boardId);
        Task<Result<BoardInvite>> GetInviteByCodeAsync(string code);
        Task<Result<bool>> RevokeInviteAsync(int inviteId);
        Task<Result<List<BoardInvite>>> GetActiveInvitesForBoardAsync(int boardId);
        Task<Result<bool>> RevokeAllInvitesForBoardAsync(int boardId);
    }

    public class BoardInviteService : IBoardInviteService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int MaxActiveInvitesPerBoard;
        private readonly int DefaultInviteExpiryHours;
        private readonly Random random;

        public BoardInviteService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            MaxActiveInvitesPerBoard = _configuration.GetValue<int>("BoardInvites:MaxActiveInvitesPerBoard");
            DefaultInviteExpiryHours = _configuration.GetValue<int>("BoardInvites:DefaultInviteExpiryHours");

            random = new Random();
        }

        public async Task<Result<BoardInvite>> GenerateInviteAsync(int boardId, int userId, BoardMemberRole role = BoardMemberRole.Member)
        {
            var canGenerateResult = await CanGenerateInviteAsync(boardId, userId);
            if (!canGenerateResult.IsSuccess || !canGenerateResult.Value)
            {
                return Result<BoardInvite>.Forbidden("You do not have permission to generate invites for this board");
            }

            var activeInvitesCount = await _context.BoardInvites
                .CountAsync(bi => bi.BoardId == boardId && !bi.IsRevoked && bi.ExpiresAt > DateTime.UtcNow);

            if (activeInvitesCount >= MaxActiveInvitesPerBoard)
            {
                return Result<BoardInvite>.Failure($"Board has reached maximum active invite limit of {MaxActiveInvitesPerBoard}", 400);
            }

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var token = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var now = DateTime.UtcNow;

            var invite = new BoardInvite
            {
                BoardId = boardId,
                Token = token,
                ExpiresAt = now.AddHours(DefaultInviteExpiryHours),
                CreatedAt = now,
                UpdatedAt = now,
                IsRevoked = false,
                HasBeenUsed = false,
                GeneratedByUserId = userId,
                Role = role
            };

            _context.BoardInvites.Add(invite);
            await _context.SaveChangesAsync();
            return Result<BoardInvite>.Success(invite);
        }

        public async Task<Result<bool>> CanGenerateInviteAsync(int boardId, int userId)
        {
            var isAdminBoardMember = await _context.BoardMembers
                .AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId && bm.Role == BoardMemberRole.Admin);

            var isOwner = await _context.Boards
                .AnyAsync(b => b.Id == boardId && b.OwnerId == userId);

            return Result<bool>.Success(isAdminBoardMember || isOwner);
        }

        public async Task<Result<bool>> ValidateInviteCodeAsync(string code, int boardId)
        {
            var invite = await _context.BoardInvites
                .FirstOrDefaultAsync(bi => bi.Token == code && bi.BoardId == boardId && !bi.IsRevoked && !bi.HasBeenUsed && bi.ExpiresAt > DateTime.UtcNow);

            return Result<bool>.Success(invite != null);
        }

        public async Task<Result<BoardInvite>> GetInviteByCodeAsync(string code)
        {
            var invite = await _context.BoardInvites
                .FirstOrDefaultAsync(bi => bi.Token == code && !bi.IsRevoked && !bi.HasBeenUsed && bi.ExpiresAt > DateTime.UtcNow);

            if (invite == null)
            {
                return Result<BoardInvite>.NotFound("Invalid or expired invite code");
            }

            return Result<BoardInvite>.Success(invite);
        }

        public async Task<Result<bool>> RevokeInviteAsync(int inviteId)
        {
            var invite = await _context.BoardInvites.FindAsync(inviteId);
            if (invite == null)
            {
                return Result<bool>.NotFound($"Invite with ID {inviteId} not found");
            }

            if (invite.IsRevoked)
            {
                return Result<bool>.Failure("Invite has already been revoked", 400);
            }

            invite.IsRevoked = true;
            invite.UpdatedAt = DateTime.UtcNow;

            _context.BoardInvites.Update(invite);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<List<BoardInvite>>> GetActiveInvitesForBoardAsync(int boardId)
        {
            var invites = await _context.BoardInvites
                .Where(bi => bi.BoardId == boardId && !bi.IsRevoked && bi.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            return Result<List<BoardInvite>>.Success(invites);
        }

        public async Task<Result<bool>> RevokeAllInvitesForBoardAsync(int boardId)
        {
            var invites = await _context.BoardInvites
                .Where(bi => bi.BoardId == boardId && !bi.IsRevoked)
                .ToListAsync();

            if (invites.Count == 0)
            {
                return Result<bool>.Failure("No active invites found for this board", 400);
            }

            foreach (var invite in invites)
            {
                invite.IsRevoked = true;
                invite.UpdatedAt = DateTime.UtcNow;
            }

            _context.BoardInvites.UpdateRange(invites);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
