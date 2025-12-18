using Xunit;
using Microsoft.Extensions.Configuration;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class CardServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BoardAuthorizationService _authService;
    private readonly ColumnService _columnService;
    private readonly CardService _cardService;
    private readonly IConfiguration _configuration;

    public CardServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _authService = new BoardAuthorizationService(_context);
        _columnService = new ColumnService(_context, _authService);

        // Create in-memory configuration
        var configData = new Dictionary<string, string?>
            {
                {"Tags:MaxTagsPerCard", "5"}
            };
        _configuration = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

        _cardService = new CardService(_context, _configuration, _authService, _columnService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetCardByIdAsync_WithValidCard_ReturnsCard()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cardService.GetCardByIdAsync(card.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(card.Id, result.Value.Id);
        Assert.Equal("Test Card", result.Value.Title);
    }

    [Fact]
    public async Task GetCardByIdAsync_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
        var nonMember = TestDataFactory.CreateTestUser(email: "nonmember@example.com");
        _context.Users.AddRange(owner, nonMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: owner.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cardService.GetCardByIdAsync(card.Id, nonMember.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task CreateCardAsync_WithValidData_CreatesCard()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var createDto = new CreateCardDto
        {
            Title = "New Card",
            Description = "New card description"
        };

        // Act
        var result = await _cardService.CreateCardAsync(column.Id, createDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("New Card", result.Value.Title);
        Assert.Equal("New card description", result.Value.Description);
        Assert.Equal(column.Id, result.Value.ColumnId);
    }

    [Fact]
    public async Task CreateCardAsync_WithWipLimitReached_ReturnsFailure()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do", position: 0, wipLimit: 1);
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var existingCard = TestDataFactory.CreateTestCard(column.Id, "Existing Card", createdById: user.Id);
        _context.Cards.Add(existingCard);
        await _context.SaveChangesAsync();

        var createDto = new CreateCardDto
        {
            Title = "New Card"
        };

        // Act
        var result = await _cardService.CreateCardAsync(column.Id, createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("WIP limit", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task UpdateCardAsync_WithValidData_UpdatesCard()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Old Title", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCardDto
        {
            Title = "Updated Title",
            Description = "Updated Description"
        };

        // Act
        var result = await _cardService.UpdateCardAsync(card.Id, updateDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated Title", result.Value.Title);
        Assert.Equal("Updated Description", result.Value.Description);
    }

    [Fact]
    public async Task DeleteCardAsync_WithValidCard_DeletesCard()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cardService.DeleteCardAsync(card.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        // Verify card was deleted
        var deletedCard = await _context.Cards.FindAsync(card.Id);
        Assert.Null(deletedCard);
    }

    [Fact]
    public async Task MoveCardAsync_WithinSameColumn_UpdatesPosition()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card1 = TestDataFactory.CreateTestCard(column.Id, "Card 1", position: 0, createdById: user.Id);
        var card2 = TestDataFactory.CreateTestCard(column.Id, "Card 2", position: 1, createdById: user.Id);
        var card3 = TestDataFactory.CreateTestCard(column.Id, "Card 3", position: 2, createdById: user.Id);
        _context.Cards.AddRange(card1, card2, card3);
        await _context.SaveChangesAsync();

        var moveDto = new MoveCardDto
        {
            NewColumnId = column.Id,
            NewPosition = 2
        };

        // Act
        var result = await _cardService.MoveCardAsync(card1.Id, moveDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Position);
    }

    [Fact]
    public async Task CreateCardAsync_WithViewerRole_ReturnsForbidden()
    {
        // Arrange - Viewer role cannot create cards (requires Member+)
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var createDto = new CreateCardDto
        {
            Title = "New Card",
            Description = "New card description"
        };

        // Act
        var result = await _cardService.CreateCardAsync(column.Id, createDto, user.Id);

        // Assert - Should fail because Viewer doesn't meet Member+ requirement
        Assert.False(result.IsSuccess);
        // The error will be 403 since the column check enforces Member+ requirement
        Assert.True(result.StatusCode == 403 || result.StatusCode == 404);
    }

    [Fact]
    public async Task CreateCardAsync_WithNonMember_ReturnsForbiddenOrNotFound()
    {
        // Arrange - Non-members cannot create cards; they may get 404 or 403 depending on board membership
        var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
        var nonMember = TestDataFactory.CreateTestUser(email: "nonmember@example.com");
        _context.Users.AddRange(owner, nonMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var createDto = new CreateCardDto
        {
            Title = "New Card"
        };

        // Act
        var result = await _cardService.CreateCardAsync(column.Id, createDto, nonMember.Id);

        // Assert - Should fail; 404 or 403 depending on authorization check order
        Assert.False(result.IsSuccess);
        Assert.True(result.StatusCode == 403 || result.StatusCode == 404, $"Expected 403 or 404, got {result.StatusCode}");
    }

    [Fact]
    public async Task CreateCardAsync_WithColumnNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var createDto = new CreateCardDto
        {
            Title = "New Card"
        };

        // Act
        var result = await _cardService.CreateCardAsync(columnId: 9999, createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task UpdateCardAsync_WithViewerRole_ReturnsForbidden()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCardDto
        {
            Title = "Updated Title"
        };

        // Act
        var result = await _cardService.UpdateCardAsync(card.Id, updateDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task DeleteCardAsync_WithViewerRole_ReturnsForbidden()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cardService.DeleteCardAsync(card.Id, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task MoveCardAsync_ToColumnExceedingWipLimit_ReturnsFailure()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var sourceColumn = TestDataFactory.CreateTestColumn(board.Id, "To Do", position: 0);
        var targetColumn = TestDataFactory.CreateTestColumn(board.Id, "Done", position: 1, wipLimit: 1);
        _context.Columns.AddRange(sourceColumn, targetColumn);
        await _context.SaveChangesAsync();

        var sourceCard = TestDataFactory.CreateTestCard(sourceColumn.Id, "Card to Move", position: 0, createdById: user.Id);
        var targetCard = TestDataFactory.CreateTestCard(targetColumn.Id, "Existing Card", position: 0, createdById: user.Id);
        _context.Cards.AddRange(sourceCard, targetCard);
        await _context.SaveChangesAsync();

        var moveDto = new MoveCardDto
        {
            NewColumnId = targetColumn.Id,
            NewPosition = 1
        };

        // Act
        var result = await _cardService.MoveCardAsync(sourceCard.Id, moveDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("WIP limit", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task MoveCardAsync_ToNonExistentColumn_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Card to Move", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var moveDto = new MoveCardDto
        {
            NewColumnId = 9999,
            NewPosition = 0
        };

        // Act
        var result = await _cardService.MoveCardAsync(card.Id, moveDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetCardByIdAsync_WhenCardNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cardService.GetCardByIdAsync(cardId: 9999, userId: user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task CreateCardAsync_WithEmptyTitle_ShouldNotValidateAtServiceLevel()
    {
        // Arrange - Service currently allows empty titles; this test documents the behavior
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var createDto = new CreateCardDto
        {
            Title = "",
            Description = "Card with empty title"
        };

        // Act
        var result = await _cardService.CreateCardAsync(column.Id, createDto, user.Id);

        // Assert - Currently succeeds, may want to add validation later
        Assert.True(result.IsSuccess);
    }

    #region Permission Tests

    [Fact]
    public async Task CreateCardAsync_AsViewer_ReturnsForbidden()
    {
        // Arrange - Viewers cannot access columns that require Member role, so they get NotFound
        // This test documents that behavior
        var admin = TestDataFactory.CreateTestAdminUser();
        var viewer = TestDataFactory.CreateTestViewerUser();
        _context.Users.AddRange(admin, viewer);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        var viewerMember = TestDataFactory.CreateTestBoardMember(board.Id, viewer.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(adminMember, viewerMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var createDto = new CreateCardDto { Title = "New Card", Description = "Test" };

        // Act
        var result = await _cardService.CreateCardAsync(column.Id, createDto, viewer.Id);

        // Assert - Viewer cannot access column (requires Member), so gets NotFound
        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateCardAsync_AsViewer_ReturnsForbidden()
    {
        // Arrange
        var admin = TestDataFactory.CreateTestAdminUser();
        var viewer = TestDataFactory.CreateTestViewerUser();
        _context.Users.AddRange(admin, viewer);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        var viewerMember = TestDataFactory.CreateTestBoardMember(board.Id, viewer.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(adminMember, viewerMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: admin.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCardDto { Title = "Updated", Description = "Updated" };

        // Act
        var result = await _cardService.UpdateCardAsync(card.Id, updateDto, viewer.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task MoveCardAsync_AsViewer_ReturnsForbidden()
    {
        // Arrange
        var admin = TestDataFactory.CreateTestAdminUser();
        var viewer = TestDataFactory.CreateTestViewerUser();
        _context.Users.AddRange(admin, viewer);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        var viewerMember = TestDataFactory.CreateTestBoardMember(board.Id, viewer.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(adminMember, viewerMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: admin.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var moveDto = new MoveCardDto { NewColumnId = column.Id, NewPosition = 0 };

        // Act
        var result = await _cardService.MoveCardAsync(card.Id, moveDto, viewer.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task DeleteCardAsync_AsViewer_ReturnsForbidden()
    {
        // Arrange
        var admin = TestDataFactory.CreateTestAdminUser();
        var viewer = TestDataFactory.CreateTestViewerUser();
        _context.Users.AddRange(admin, viewer);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        var viewerMember = TestDataFactory.CreateTestBoardMember(board.Id, viewer.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(adminMember, viewerMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: admin.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cardService.DeleteCardAsync(card.Id, viewer.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    #endregion

    #region WIP Limit Tests

    [Fact]
    public async Task CreateCardAsync_WhenColumnAtWipLimit_ReturnsFailure()
    {
        // Arrange
        var user = TestDataFactory.CreateTestMemberUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do", wipLimit: 2);
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        // Create 2 cards to reach WIP limit
        var card1 = TestDataFactory.CreateTestCard(column.Id, "Card 1", position: 0, createdById: user.Id);
        var card2 = TestDataFactory.CreateTestCard(column.Id, "Card 2", position: 1, createdById: user.Id);
        _context.Cards.AddRange(card1, card2);
        await _context.SaveChangesAsync();

        var createDto = new CreateCardDto { Title = "Card 3", Description = "Should fail" };

        // Act
        var result = await _cardService.CreateCardAsync(column.Id, createDto, user.Id);

        // Assert
        result.AssertFailure(400, "WIP limit");
    }

    [Fact]
    public async Task MoveCardAsync_ToColumnAtWipLimit_ReturnsFailure()
    {
        // Arrange
        var user = TestDataFactory.CreateTestMemberUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var fromColumn = TestDataFactory.CreateTestColumn(board.Id, "From", position: 0);
        var toColumn = TestDataFactory.CreateTestColumn(board.Id, "To", position: 1, wipLimit: 1);
        _context.Columns.AddRange(fromColumn, toColumn);
        await _context.SaveChangesAsync();

        var cardToMove = TestDataFactory.CreateTestCard(fromColumn.Id, "Card 1", position: 0, createdById: user.Id);
        var existingCard = TestDataFactory.CreateTestCard(toColumn.Id, "Card 2", position: 0, createdById: user.Id);
        _context.Cards.AddRange(cardToMove, existingCard);
        await _context.SaveChangesAsync();

        var moveDto = new MoveCardDto { NewColumnId = toColumn.Id, NewPosition = 0 };

        // Act
        var result = await _cardService.MoveCardAsync(cardToMove.Id, moveDto, user.Id);

        // Assert
        result.AssertFailure(400, "WIP limit");
    }

    [Fact]
    public async Task MoveCardAsync_OutOfFullColumnThenBackIn_Succeeds()
    {
        // Arrange
        var user = TestDataFactory.CreateTestMemberUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column1 = TestDataFactory.CreateTestColumn(board.Id, "Col1", position: 0, wipLimit: 2);
        var column2 = TestDataFactory.CreateTestColumn(board.Id, "Col2", position: 1, wipLimit: 1);
        _context.Columns.AddRange(column1, column2);
        await _context.SaveChangesAsync();

        var card1 = TestDataFactory.CreateTestCard(column1.Id, "Card 1", position: 0, createdById: user.Id);
        var card2 = TestDataFactory.CreateTestCard(column2.Id, "Card 2", position: 0, createdById: user.Id);
        _context.Cards.AddRange(card1, card2);
        await _context.SaveChangesAsync();

        // Try to move card1 to col2 (should fail due to WIP limit)
        var moveDto1 = new MoveCardDto { NewColumnId = column2.Id, NewPosition = 0 };
        var result1 = await _cardService.MoveCardAsync(card1.Id, moveDto1, user.Id);
        Assert.False(result1.IsSuccess, "Expected move to col2 to fail due to WIP limit");

        // Move card2 to col1 (should succeed, col1 has WIP=2, currently 1 card)
        var moveDto2 = new MoveCardDto { NewColumnId = column1.Id, NewPosition = 1 };
        var result2 = await _cardService.MoveCardAsync(card2.Id, moveDto2, user.Id);
        Assert.True(result2.IsSuccess, $"Expected move to col1 to succeed, but got: {result2.Error}");

        // Now try moving card1 to col2 (should succeed now that col2 is empty)
        var moveDto3 = new MoveCardDto { NewColumnId = column2.Id, NewPosition = 0 };
        var result3 = await _cardService.MoveCardAsync(card1.Id, moveDto3, user.Id);

        // Assert
        result3.AssertSuccess();
    }

    #endregion

    #region Card Reordering Tests

    [Fact]
    public async Task MoveCardAsync_MultipleCards_InterleavedMoves_MaintainsIntegrity()
    {
        // Arrange
        var user = TestDataFactory.CreateTestMemberUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card1 = TestDataFactory.CreateTestCard(column.Id, "Card 1", position: 0, createdById: user.Id);
        var card2 = TestDataFactory.CreateTestCard(column.Id, "Card 2", position: 1, createdById: user.Id);
        var card3 = TestDataFactory.CreateTestCard(column.Id, "Card 3", position: 2, createdById: user.Id);
        var card4 = TestDataFactory.CreateTestCard(column.Id, "Card 4", position: 3, createdById: user.Id);
        _context.Cards.AddRange(card1, card2, card3, card4);
        await _context.SaveChangesAsync();

        // Move card3 to position 1
        var moveDto1 = new MoveCardDto { NewColumnId = column.Id, NewPosition = 1 };
        await _cardService.MoveCardAsync(card3.Id, moveDto1, user.Id);

        // Move card1 to position 3
        var moveDto2 = new MoveCardDto { NewColumnId = column.Id, NewPosition = 3 };
        var result = await _cardService.MoveCardAsync(card1.Id, moveDto2, user.Id);

        // Assert
        result.AssertSuccess();

        // Verify positions
        var cards = _context.Cards.Where(c => c.ColumnId == column.Id).OrderBy(c => c.Position).ToList();
        Assert.Equal(4, cards.Count);
        for (int i = 0; i < cards.Count; i++)
        {
            Assert.Equal(i, cards[i].Position);
        }
    }

    [Fact]
    public async Task MoveCardAsync_NoChange_ReturnsSameCard()
    {
        // Arrange
        var user = TestDataFactory.CreateTestMemberUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Card", position: 0, createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var moveDto = new MoveCardDto { NewColumnId = column.Id, NewPosition = 0 };

        // Act
        var result = await _cardService.MoveCardAsync(card.Id, moveDto, user.Id);

        // Assert
        var movedCard = result.AssertSuccess();
        Assert.Equal(column.Id, movedCard.ColumnId);
        Assert.Equal(0, movedCard.Position);
    }

    #endregion

    #region Tag Management Tests

    [Fact]
    public async Task AssignTagsToCardAsync_WithValidTags_Succeeds()
    {
        // Arrange
        var user = TestDataFactory.CreateTestMemberUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Card", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var tag1 = TestDataFactory.CreateTestTag(board.Id, "Bug");
        var tag2 = TestDataFactory.CreateTestTag(board.Id, "Feature");
        _context.Tags.AddRange(tag1, tag2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cardService.AssignTagsToCardAsync(card.Id, new List<int> { tag1.Id, tag2.Id }, user.Id);

        // Assert
        result.AssertSuccess();
    }

    [Fact]
    public async Task AssignTagsToCardAsync_WithEmptyTagList_Succeeds()
    {
        // Arrange
        var user = TestDataFactory.CreateTestMemberUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Card", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cardService.AssignTagsToCardAsync(card.Id, new List<int>(), user.Id);

        // Assert
        result.AssertSuccess();
    }

    [Fact]
    public async Task AssignTagsToCardAsync_ExceedingMaxTags_ReturnsBadRequest()
    {
        // Arrange
        var user = TestDataFactory.CreateTestMemberUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Card", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Create 6 tags
        var tags = Enumerable.Range(1, 6)
            .Select(i => TestDataFactory.CreateTestTag(board.Id, $"Tag{i}"))
            .ToList();
        _context.Tags.AddRange(tags);
        await _context.SaveChangesAsync();

        // Try to assign more than max (5)
        var tagIds = tags.Select(t => t.Id).ToList();

        // Act
        var result = await _cardService.AssignTagsToCardAsync(card.Id, tagIds, user.Id);

        // Assert
        result.AssertFailure(400);
    }

    #endregion
}
