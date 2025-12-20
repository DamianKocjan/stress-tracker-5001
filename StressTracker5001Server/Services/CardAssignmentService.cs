using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface ICardAssignmentService
    {
        Task<Result<bool>> AssignCardToUserAsync(int cardId, int userId, int assignedUserId);
        Task<Result<bool>> UnassignCardFromUserAsync(int cardId, int userId, int assignedUserId);
        Task<Result<List<Card>>> GetCardsAssignedToUserAsync(int boardId, int userId, int assignedUserId);
    }

    public class CardAssignmentService : ICardAssignmentService
    {
        private readonly AppDbContext _context;
        private readonly IBoardAuthorizationService _boardAuthorizationService;
        private readonly ICardService _cardService;
        private readonly IActivityLogService _activityLogService;

        public CardAssignmentService(AppDbContext context, IBoardAuthorizationService boardAuthorizationService, ICardService cardService, IActivityLogService activityLogService)
        {
            _context = context;
            _boardAuthorizationService = boardAuthorizationService;
            _cardService = cardService;
            _activityLogService = activityLogService;
        }

        public async Task<Result<bool>> AssignCardToUserAsync(int cardId, int userId, int assignedUserId)
        {
            var cardResult = await _cardService.GetCardByIdAsync(cardId, userId, BoardMemberRole.Member);
            if (!cardResult.IsSuccess)
            {
                return Result<bool>.Failure(cardResult.Error!, cardResult.StatusCode);
            }

            var card = cardResult.Value;

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(card!.Column!.BoardId, assignedUserId))
            {
                return Result<bool>.Forbidden("User you are trying to assign the card to is not authorized to access this board");
            }

            var existingAssignment = await _context.CardAssignments
                .FirstOrDefaultAsync(ca => ca.CardId == cardId && ca.UserId == assignedUserId);
            if (existingAssignment != null)
            {
                return Result<bool>.Failure("User is already assigned to this card", 400);
            }
            var cardAssignment = new CardAssignment
            {
                CardId = cardId,
                UserId = assignedUserId,
                AssignedAt = DateTime.UtcNow
            };
            _context.CardAssignments.Add(cardAssignment);
            await _context.SaveChangesAsync();

            var assignedUser = await _context.Users.FindAsync(assignedUserId);
            await _activityLogService.LogCardAssignedAsync(card!.Column!.BoardId, userId, cardId, assignedUserId, assignedUser?.Username ?? "Unknown");

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> UnassignCardFromUserAsync(int cardId, int userId, int assignedUserId)
        {
            var cardResult = await _cardService.GetCardByIdAsync(cardId, userId, BoardMemberRole.Member);
            if (!cardResult.IsSuccess)
            {
                return Result<bool>.Failure(cardResult.Error!, cardResult.StatusCode);
            }

            var card = cardResult.Value;

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(card!.Column!.BoardId, assignedUserId))
            {
                return Result<bool>.Forbidden("User you are trying to unassign the card from is not authorized to access this board");
            }

            var assignment = await _context.CardAssignments
                .FirstOrDefaultAsync(ca => ca.CardId == cardId && ca.UserId == assignedUserId);
            if (assignment == null)
            {
                return Result<bool>.NotFound("Card assignment not found");
            }

            _context.CardAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            var unassignedUser = await _context.Users.FindAsync(assignedUserId);
            await _activityLogService.LogCardUnassignedAsync(card!.Column!.BoardId, userId, cardId, assignedUserId, unassignedUser?.Username ?? "Unknown");

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<Card>>> GetCardsAssignedToUserAsync(int boardId, int userId, int assignedUserId)
        {
            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(boardId, userId, BoardMemberRole.Viewer))
            {
                return Result<List<Card>>.Forbidden("User is not authorized to view cards on this board");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(boardId, assignedUserId, BoardMemberRole.Viewer))
            {
                return Result<List<Card>>.Forbidden("Assigned user is not authorized to view cards on this board");
            }

            var assignedCards = await _context.CardAssignments
                .Where(ca => ca.UserId == assignedUserId && ca.Card.Column.Board.Id == boardId)
                .Include(ca => ca.Card)
                .ThenInclude(c => c.Column)
                .ThenInclude(c => c.Board)
                .Include(ca => ca.Card)
                .ThenInclude(c => c.CardTags)
                .ThenInclude(ct => ct.Tag)
                .Include(ca => ca.Card)
                .ThenInclude(c => c.CardAssignments)
                .ThenInclude(ca => ca.User)
                .Select(ca => ca.Card!)
                .ToListAsync();

            return Result<List<Card>>.Success(assignedCards);
        }
    }
}
