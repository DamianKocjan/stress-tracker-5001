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

        public CardAssignmentService(AppDbContext context, IBoardAuthorizationService boardAuthorizationService, ICardService cardService)
        {
            _context = context;
            _boardAuthorizationService = boardAuthorizationService;
            _cardService = cardService;
        }

        public async Task<Result<bool>> AssignCardToUserAsync(int cardId, int userId, int assignedUserId)
        {
            var cardResult = await _cardService.GetCardByIdAsync(cardId, userId, BoardMemberRole.Member);
            if (!cardResult.IsSuccess)
            {
                return Result<bool>.Failure(cardResult.Error!, cardResult.StatusCode);
            }

            var card = cardResult.Value;

            // Check if the assigned user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == assignedUserId);
            if (!userExists)
            {
                return Result<bool>.NotFound("User to assign the card to was not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(cardId, assignedUserId, BoardMemberRole.Member))
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
                UserId = assignedUserId
            };
            _context.CardAssignments.Add(cardAssignment);
            await _context.SaveChangesAsync();

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

            // Check if the assigned user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == assignedUserId);
            if (!userExists)
            {
                return Result<bool>.NotFound("User to unassign the card from was not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(cardId, assignedUserId, BoardMemberRole.Member))
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
                .Where(ca => ca.UserId == assignedUserId)
                .Include(ca => ca.Card)
                .Where(ca => ca.Card != null && ca.Card.Column != null && ca.Card.Column.Board != null && ca.Card.Column.Board.Id == boardId)
                .Select(ca => ca.Card!)
                .ToListAsync();

            return Result<List<Card>>.Success(assignedCards);
        }
    }
}
