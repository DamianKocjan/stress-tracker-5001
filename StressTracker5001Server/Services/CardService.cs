using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.Models;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Services
{
    public interface ICardService
    {
        Task<Result<Card>> GetCardByIdAsync(int cardId, int userId, BoardMemberRole requiredRole = BoardMemberRole.Viewer);
        Task<Result<Card>> GetCardDetailsByIdAsync(int cardId, int userId);
        Task<Result<Card>> CreateCardAsync(int columnId, CreateCardDto dto, int userId);
        Task<Result<Card>> UpdateCardAsync(int cardId, UpdateCardDto dto, int userId);
        Task<Result<Card>> MoveCardAsync(int cardId, MoveCardDto dto, int userId);
        Task<Result<Card>> AssignTagsToCardAsync(int cardId, List<int> tagIds, int userId);
        Task<Result<List<Comment>>> GetCommentsByCardIdAsync(int cardId, int userId, int page, int pageSize);
        Task<Result<bool>> HasMoreCommentsAsync(int cardId, int userId, int page, int pageSize);
        Task<Result<Comment>> AddCommentToCardAsync(int cardId, CreateCommentDto dto, int userId);
        Task<Result<bool>> DeleteCardAsync(int cardId, int userId);
    }

    public class CardService : ICardService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IBoardAuthorizationService _boardAuthorizationService;
        private readonly IColumnService _columnService;
        private readonly IActivityLogService _activityLogService;

        private readonly int _maxTagsPerCard;

        public CardService(AppDbContext context, IConfiguration configuration, IBoardAuthorizationService boardAuthorizationService, IColumnService columnService, IActivityLogService activityLogService)
        {
            _context = context;
            _configuration = configuration;
            _boardAuthorizationService = boardAuthorizationService;
            _columnService = columnService;
            _activityLogService = activityLogService;

            _maxTagsPerCard = _configuration.GetValue("Tags:MaxTagsPerCard", 5);
        }

        public async Task<Result<Card>> GetCardByIdAsync(int cardId, int userId, BoardMemberRole requiredRole = BoardMemberRole.Viewer)
        {
            var card = await _context.Cards
                .Include(c => c.Column)
                .ThenInclude(c => c!.Board)
                .Include(c => c.CardTags)
                .Include(c => c.CardAssignments)
                .ThenInclude(ca => ca.User)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
            {
                return Result<Card>.NotFound($"Card with ID {cardId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(card.Column!.BoardId, userId, requiredRole))
            {
                return Result<Card>.Forbidden("You do not have permission to access this card");
            }

            return Result<Card>.Success(card);
        }

        public async Task<Result<Card>> GetCardDetailsByIdAsync(int cardId, int userId)
        {
            var card = await _context.Cards
                .Include(c => c.Column)
                .ThenInclude(c => c!.Board)
                .Include(c => c.CreatedBy)
                .Include(c => c.CardTags)
                .Include(c => c.CardAssignments)
                .ThenInclude(ca => ca.User)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
            {
                return Result<Card>.NotFound($"Card with ID {cardId} not found");
            }

            if (!await _boardAuthorizationService.UserCanAccessBoardAsync(card.Column!.BoardId, userId))
            {
                return Result<Card>.Forbidden("You do not have permission to access this card");
            }

            return Result<Card>.Success(card);
        }

        public async Task<Result<Card>> CreateCardAsync(int columnId, CreateCardDto dto, int userId)
        {
            // Validate column exists and user has access
            var columnResult = await _columnService.GetColumnByIdAsync(columnId, userId, BoardMemberRole.Member);

            if (!columnResult.IsSuccess)
            {
                return Result<Card>.NotFound(columnResult.Error ?? "Column not found");
            }

            var column = columnResult.Value!;
            var cardCount = await _context.Cards.CountAsync(c => c.ColumnId == columnId);

            // Check WIP limit
            if (column.WipLimit != null && cardCount >= column.WipLimit)
            {
                return Result<Card>.Failure($"Cannot create card. Column '{column.Name}' has reached WIP limit of {column.WipLimit}", 400);
            }

            var now = DateTime.UtcNow;

            var card = new Card
            {
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                DueDate = dto.DueDate,
                ColumnId = columnId,
                CreatedById = userId,
                Position = cardCount,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            await _activityLogService.LogCardCreatedAsync(column.BoardId, userId, card.Id, card.Title);

            return Result<Card>.Success(card);
        }

        public async Task<Result<Card>> UpdateCardAsync(int cardId, UpdateCardDto dto, int userId)
        {
            var cardResult = await GetCardByIdAsync(cardId, userId, BoardMemberRole.Member);
            if (!cardResult.IsSuccess)
            {
                return cardResult;
            }

            var card = cardResult.Value!;
            var oldCard = new { card.Title, card.Description, card.DueDate };

            card.Title = dto.Title;
            card.Description = dto.Description ?? string.Empty;
            card.DueDate = dto.DueDate;
            card.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLogService.LogCardUpdatedAsync(card.Column!.BoardId, userId, cardId, oldCard, new { Title = card.Title, Description = card.Description, DueDate = card.DueDate });

            return Result<Card>.Success(card);
        }

        public async Task<Result<Card>> MoveCardAsync(int cardId, MoveCardDto dto, int userId)
        {
            var cardResult = await GetCardByIdAsync(cardId, userId, BoardMemberRole.Member);
            if (!cardResult.IsSuccess)
            {
                return cardResult.StatusCode switch
                {
                    403 => Result<Card>.Forbidden(cardResult.Error ?? "Forbidden"),
                    404 => Result<Card>.NotFound(cardResult.Error ?? "Not found"),
                    _ => Result<Card>.Failure(cardResult.Error ?? "Error", cardResult.StatusCode)
                };
            }

            var card = cardResult.Value!;
            var oldColumnId = card.ColumnId;
            var oldPosition = card.Position;

            if (oldColumnId == dto.NewColumnId && oldPosition == dto.NewPosition)
            {
                return Result<Card>.Success(card); // No change needed
            }

            if (oldColumnId == dto.NewColumnId)
            {
                // Moving within the same column
                var cardsInColumn = await _context.Cards
                    .Where(c => c.ColumnId == oldColumnId && c.Id != card.Id)
                    .OrderBy(c => c.Position)
                    .ToListAsync();

                cardsInColumn.Insert(dto.NewPosition, card);
                for (int i = 0; i < cardsInColumn.Count; i++)
                {
                    cardsInColumn[i].Position = i;
                    cardsInColumn[i].UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                await _activityLogService.LogCardMovedAsync(card.Column!.BoardId, userId, cardId, oldColumnId, dto.NewColumnId);

                return Result<Card>.Success(card);
            }

            // Moving to a different column - validate target column
            var columnResult = await _columnService.GetColumnByIdAsync(dto.NewColumnId, userId, BoardMemberRole.Member);
            if (!columnResult.IsSuccess)
            {
                return Result<Card>.NotFound(columnResult.Error ?? "Target column not found");
            }

            var column = columnResult.Value!;

            // Check WIP limit
            if (column.WipLimit != null)
            {
                var cardCountInTargetColumn = await _context.Cards
                    .Where(c => c.ColumnId == dto.NewColumnId && c.Id != card.Id)
                    .CountAsync();

                if (cardCountInTargetColumn >= column.WipLimit)
                {
                    return Result<Card>.Failure($"Cannot move card. Column '{column.Name}' has reached WIP limit of {column.WipLimit}", 400);
                }
            }

            card.Position = dto.NewPosition;
            card.ColumnId = dto.NewColumnId;
            card.UpdatedAt = DateTime.UtcNow;

            // Update the positions of cards in the old column
            var oldColumnCards = await _context.Cards
                .Where(c => c.ColumnId == oldColumnId && c.Id != card.Id)
                .OrderBy(c => c.Position)
                .ToListAsync();
            for (int i = 0; i < oldColumnCards.Count; i++)
            {
                oldColumnCards[i].Position = i;
                oldColumnCards[i].UpdatedAt = DateTime.UtcNow;
            }

            // Update the positions of other cards in the new column
            var newColumnCards = await _context.Cards
                .Where(c => c.ColumnId == dto.NewColumnId && c.Id != card.Id)
                .OrderBy(c => c.Position)
                .ToListAsync();

            newColumnCards.Insert(dto.NewPosition, card);
            for (int i = 0; i < newColumnCards.Count; i++)
            {
                newColumnCards[i].Position = i;
                newColumnCards[i].UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _activityLogService.LogCardMovedAsync(card.Column!.BoardId, userId, cardId, oldColumnId, dto.NewColumnId);

            return Result<Card>.Success(card);
        }

        public async Task<Result<Card>> AssignTagsToCardAsync(int cardId, List<int> tagIds, int userId)
        {
            var cardResult = await GetCardByIdAsync(cardId, userId, BoardMemberRole.Member);
            if (!cardResult.IsSuccess)
            {
                return Result<Card>.NotFound(cardResult.Error ?? "Card not found");
            }

            var card = cardResult.Value!;

            // Get existing tag IDs
            var existingTagIds = card.CardTags.Select(ct => ct.TagId).ToHashSet();

            // Check if adding new tags would exceed the maximum limit
            var totalTagsCount = tagIds.Union(existingTagIds).Count();
            if (totalTagsCount > _maxTagsPerCard)
            {
                return Result<Card>.Failure($"Cannot assign tags. Maximum {_maxTagsPerCard} tags per card allowed", 400);
            }

            // Add new tags
            foreach (var tagId in tagIds.Where(id => !existingTagIds.Contains(id)))
            {
                card.CardTags.Add(new CardTag
                {
                    CardId = card.Id,
                    TagId = tagId
                });
            }

            // Remove tags that are no longer assigned
            var tagsToRemove = existingTagIds.Except(tagIds).ToHashSet();
            if (tagsToRemove.Count != 0)
            {
                var cardTagsToRemove = card.CardTags
                    .Where(ct => tagsToRemove.Contains(ct.TagId))
                    .ToList();

                foreach (var cardTag in cardTagsToRemove)
                {
                    _context.CardTags.Remove(cardTag);
                }
            }

            card.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<Card>.Success(card);
        }

        public async Task<Result<List<Comment>>> GetCommentsByCardIdAsync(int cardId, int userId, int page, int pageSize)
        {
            var cardResult = await GetCardByIdAsync(cardId, userId);
            if (!cardResult.IsSuccess)
            {
                return Result<List<Comment>>.NotFound(cardResult.Error ?? "Card not found");
            }

            var offset = (page - 1) * pageSize;

            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.CardId == cardId)
                .OrderBy(c => c.CreatedAt)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();

            return Result<List<Comment>>.Success(comments);
        }

        public async Task<Result<bool>> HasMoreCommentsAsync(int cardId, int userId, int page, int pageSize)
        {
            var cardResult = await GetCardByIdAsync(cardId, userId);
            if (!cardResult.IsSuccess)
            {
                return Result<bool>.NotFound(cardResult.Error ?? "Card not found");
            }

            var totalComments = await _context.Comments
                .Where(c => c.CardId == cardId)
                .CountAsync();

            var fetchedComments = page * pageSize;
            var hasMore = fetchedComments < totalComments;

            return Result<bool>.Success(hasMore);
        }

        public async Task<Result<Comment>> AddCommentToCardAsync(int cardId, CreateCommentDto dto, int userId)
        {
            var cardResult = await GetCardByIdAsync(cardId, userId, BoardMemberRole.Member);

            if (!cardResult.IsSuccess)
            {
                return Result<Comment>.NotFound($"Card with ID {cardId} not found or access denied");
            }

            var now = DateTime.UtcNow;
            var comment = new Comment
            {
                CardId = cardId,
                UserId = userId,
                User = _context.Users.Find(userId)!,
                Content = dto.Content,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            await _activityLogService.LogCommentCreatedAsync(cardResult.Value!.Column!.BoardId, userId, comment.Id, cardId, comment.Content);

            return Result<Comment>.Success(comment);
        }

        public async Task<Result<bool>> DeleteCardAsync(int cardId, int userId)
        {
            var cardResult = await GetCardByIdAsync(cardId, userId, BoardMemberRole.Member);
            if (!cardResult.IsSuccess)
            {
                return cardResult.StatusCode switch
                {
                    403 => Result<bool>.Forbidden(cardResult.Error ?? "Forbidden"),
                    404 => Result<bool>.NotFound(cardResult.Error ?? "Not found"),
                    _ => Result<bool>.Failure(cardResult.Error ?? "Error", cardResult.StatusCode)
                };
            }

            var card = cardResult.Value!;
            var cardTitle = card.Title;
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            await _activityLogService.LogCardDeletedAsync(card.Column!.BoardId, userId, cardId, cardTitle);

            return Result<bool>.Success(true);
        }
    }
}
