using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;
using System.Security.Cryptography;

namespace StressTracker5001Server.Services
{
    public interface IBoardInviteService
    {
        Task<Result<BoardInvite>> GenerateInviteAsync(int boardId, int userId, BoardMemberRole role = BoardMemberRole.Member);
        Task<Result<bool>> CanGenerateInviteAsync(int boardId, int userId);
        Result<bool> ValidateInviteCodeAsync(BoardInvite invite);
        Task<BoardInvite?> GetInviteByCodeAsync(string code);
        Task<Result<bool>> RevokeInviteAsync(int inviteId);
        Task<Result<List<BoardInvite>>> GetActiveInvitesForBoardAsync(int boardId);
        Task<Result<bool>> RevokeAllInvitesForBoardAsync(int boardId);
    }

    public class BoardInviteService : IBoardInviteService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IBoardAuthorizationService _boardAuthorizationService;

        private readonly int MaxActiveInvitesPerBoard;
        private readonly int DefaultInviteExpiryHours;
        private readonly string InviteChars;
        private readonly int InviteTokenLength;
        private readonly RandomNumberGenerator random;

        public BoardInviteService(AppDbContext context, IConfiguration configuration, IBoardAuthorizationService boardAuthorizationService)
        {
            _context = context;
            _configuration = configuration;
            _boardAuthorizationService = boardAuthorizationService;

            MaxActiveInvitesPerBoard = _configuration.GetValue<int>("BoardInvites:MaxActiveInvitesPerBoard");
            DefaultInviteExpiryHours = _configuration.GetValue<int>("BoardInvites:DefaultInviteExpiryHours");
            InviteChars = _configuration.GetValue<string>("BoardInvites:InviteChars");
            InviteTokenLength = _configuration.GetValue<int>("BoardInvites:InviteTokenLength");

            random = RandomNumberGenerator.Create();
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

            var token = new char[InviteTokenLength];
            var tokenBytes = new byte[InviteTokenLength];

            random.GetBytes(tokenBytes);
            for (int i = 0; i < token.Length; i++)
            {
                token[i] = InviteChars[tokenBytes[i] % InviteChars.Length];
            }

            var now = DateTime.UtcNow;

            var invite = new BoardInvite
            {
                BoardId = boardId,
                Token = new string(token),
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
            var isBoardMemberResult = await _boardAuthorizationService.UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Admin);
            if (!isBoardMemberResult)
            {
                return Result<bool>.Forbidden("User does not have permission to generate invites for this board");
            }
            return Result<bool>.Success(true);
        }

        public Result<bool> ValidateInviteCodeAsync(BoardInvite invite)
        {
            if (invite.IsRevoked)
            {
                return Result<bool>.Failure("Invite has been revoked", 400);
            }

            if (invite.HasBeenUsed)
            {
                return Result<bool>.Failure("Invite has already been used", 400);
            }

            if (invite.ExpiresAt <= DateTime.UtcNow)
            {
                return Result<bool>.Failure("Invite has expired", 400);
            }

            return Result<bool>.Success(true);
        }

        public async Task<BoardInvite?> GetInviteByCodeAsync(string code)
        {
            return await _context.BoardInvites
                .FirstOrDefaultAsync(bi => bi.Token == code);
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
