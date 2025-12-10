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
}
