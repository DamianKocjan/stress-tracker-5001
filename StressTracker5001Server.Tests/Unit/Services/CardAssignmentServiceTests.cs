using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class CardAssignmentServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BoardAuthorizationService _authService;
    private readonly ColumnService _columnService;
    private readonly CardService _cardService;
    private readonly CardAssignmentService _assignmentService;
    private readonly IConfiguration _configuration;

    public CardAssignmentServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _authService = new BoardAuthorizationService(_context);
        _columnService = new ColumnService(_context, _authService);

        var configData = new Dictionary<string, string?>
        {
            {"Tags:MaxTagsPerCard", "5"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _cardService = new CardService(_context, _configuration, _authService, _columnService);
        _assignmentService = new CardAssignmentService(_context, _authService, _cardService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region AssignCardToUserAsync Tests

    [Fact]
    public async Task AssignCardToUserAsync_WithValidAssignment_Succeeds()
    {
        // Arrange
        var assigningUser = TestDataFactory.CreateTestUser(email: "assigner@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(assigningUser, assignedUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(assigningUser.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var assigningMember = TestDataFactory.CreateTestBoardMember(board.Id, assigningUser.Id, BoardMemberRole.Member);
        var assignedMember = TestDataFactory.CreateTestBoardMember(board.Id, assignedUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(assigningMember, assignedMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: assigningUser.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act
        var result = await _assignmentService.AssignCardToUserAsync(card.Id, assigningUser.Id, assignedUser.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        var assignment = await _context.CardAssignments.FirstOrDefaultAsync(ca => ca.CardId == card.Id && ca.UserId == assignedUser.Id);
        Assert.NotNull(assignment);
        Assert.Equal(card.Id, assignment.CardId);
        Assert.Equal(assignedUser.Id, assignment.UserId);
    }

    [Fact]
    public async Task AssignCardToUserAsync_WithoutBoardPermission_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
        var nonMember = TestDataFactory.CreateTestUser(email: "nonmember@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(owner, nonMember, assignedUser);
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
        var result = await _assignmentService.AssignCardToUserAsync(card.Id, nonMember.Id, assignedUser.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AssignCardToUserAsync_WithNonExistentCard_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(user, assignedUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var member = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _assignmentService.AssignCardToUserAsync(cardId: 9999, user.Id, assignedUser.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task AssignCardToUserAsync_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var member = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(member);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: user.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act
        var result = await _assignmentService.AssignCardToUserAsync(card.Id, user.Id, assignedUserId: 9999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("not found", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignCardToUserAsync_WithDuplicateAssignment_ReturnsFailure()
    {
        // Arrange
        var assigningUser = TestDataFactory.CreateTestUser(email: "assigner@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(assigningUser, assignedUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(assigningUser.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var assigningMember = TestDataFactory.CreateTestBoardMember(board.Id, assigningUser.Id, BoardMemberRole.Member);
        var assignedMember = TestDataFactory.CreateTestBoardMember(board.Id, assignedUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(assigningMember, assignedMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: assigningUser.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // First assignment
        var firstAssignment = new CardAssignment
        {
            CardId = card.Id,
            UserId = assignedUser.Id,
            AssignedAt = DateTime.UtcNow
        };
        _context.CardAssignments.Add(firstAssignment);
        await _context.SaveChangesAsync();

        // Act - Try to assign same user again
        var result = await _assignmentService.AssignCardToUserAsync(card.Id, assigningUser.Id, assignedUser.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("already assigned", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignCardToUserAsync_WithViewerRole_ReturnsForbidden()
    {
        // Arrange - Viewer role cannot assign cards (requires Member+)
        var assigningUser = TestDataFactory.CreateTestUser(email: "viewer@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(assigningUser, assignedUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(assigningUser.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var viewerMember = TestDataFactory.CreateTestBoardMember(board.Id, assigningUser.Id, BoardMemberRole.Viewer);
        var assignedMember = TestDataFactory.CreateTestBoardMember(board.Id, assignedUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(viewerMember, assignedMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: assigningUser.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act
        var result = await _assignmentService.AssignCardToUserAsync(card.Id, assigningUser.Id, assignedUser.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    #endregion

    #region UnassignCardFromUserAsync Tests

    [Fact]
    public async Task UnassignCardFromUserAsync_WithValidAssignment_Succeeds()
    {
        // Arrange
        var assigningUser = TestDataFactory.CreateTestUser(email: "assigner@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(assigningUser, assignedUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(assigningUser.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var assigningMember = TestDataFactory.CreateTestBoardMember(board.Id, assigningUser.Id, BoardMemberRole.Member);
        var assignedMember = TestDataFactory.CreateTestBoardMember(board.Id, assignedUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(assigningMember, assignedMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: assigningUser.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var assignment = new CardAssignment
        {
            CardId = card.Id,
            UserId = assignedUser.Id,
            AssignedAt = DateTime.UtcNow
        };
        _context.CardAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _assignmentService.UnassignCardFromUserAsync(card.Id, assigningUser.Id, assignedUser.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        var removedAssignment = await _context.CardAssignments.FirstOrDefaultAsync(ca => ca.CardId == card.Id && ca.UserId == assignedUser.Id);
        Assert.Null(removedAssignment);
    }

    [Fact]
    public async Task UnassignCardFromUserAsync_WithoutBoardPermission_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
        var nonMember = TestDataFactory.CreateTestUser(email: "nonmember@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(owner, nonMember, assignedUser);
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
        var result = await _assignmentService.UnassignCardFromUserAsync(card.Id, nonMember.Id, assignedUser.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task UnassignCardFromUserAsync_WithNonExistentAssignment_ReturnsNotFound()
    {
        // Arrange
        var assigningUser = TestDataFactory.CreateTestUser(email: "assigner@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(assigningUser, assignedUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(assigningUser.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var assigningMember = TestDataFactory.CreateTestBoardMember(board.Id, assigningUser.Id, BoardMemberRole.Member);
        var assignedMember = TestDataFactory.CreateTestBoardMember(board.Id, assignedUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(assigningMember, assignedMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: assigningUser.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act - No assignment exists
        var result = await _assignmentService.UnassignCardFromUserAsync(card.Id, assigningUser.Id, assignedUser.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("not found", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region GetCardsAssignedToUserAsync Tests

    [Fact]
    public async Task GetCardsAssignedToUserAsync_WithValidUser_ReturnsAssignedCards()
    {
        // Arrange
        var currentUser = TestDataFactory.CreateTestUser(email: "current@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(currentUser, assignedUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(currentUser.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var currentMember = TestDataFactory.CreateTestBoardMember(board.Id, currentUser.Id, BoardMemberRole.Viewer);
        var assignedMember = TestDataFactory.CreateTestBoardMember(board.Id, assignedUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(currentMember, assignedMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card1 = TestDataFactory.CreateTestCard(column.Id, "Card 1", createdById: currentUser.Id);
        var card2 = TestDataFactory.CreateTestCard(column.Id, "Card 2", createdById: currentUser.Id);
        _context.Cards.AddRange(card1, card2);
        await _context.SaveChangesAsync();

        var assignment1 = new CardAssignment { CardId = card1.Id, UserId = assignedUser.Id, AssignedAt = DateTime.UtcNow };
        var assignment2 = new CardAssignment { CardId = card2.Id, UserId = assignedUser.Id, AssignedAt = DateTime.UtcNow };
        _context.CardAssignments.AddRange(assignment1, assignment2);
        await _context.SaveChangesAsync();

        // Act
        // Act
        var result = await _assignmentService.GetCardsAssignedToUserAsync(board.Id, currentUser.Id, assignedUser.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, c => c.Id == card1.Id);
        Assert.Contains(result.Value, c => c.Id == card2.Id);
    }

    [Fact]
    public async Task GetCardsAssignedToUserAsync_WithNonExistentUser_ReturnsForbidden()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var member = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _assignmentService.GetCardsAssignedToUserAsync(board.Id, user.Id, assignedUserId: 9999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task GetCardsAssignedToUserAsync_WithNoAssignments_ReturnsEmptyList()
    {
        // Arrange
        var currentUser = TestDataFactory.CreateTestUser(email: "current@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(currentUser, assignedUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(currentUser.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var currentMember = TestDataFactory.CreateTestBoardMember(board.Id, currentUser.Id, BoardMemberRole.Viewer);
        var assignedMember = TestDataFactory.CreateTestBoardMember(board.Id, assignedUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(currentMember, assignedMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _assignmentService.GetCardsAssignedToUserAsync(board.Id, currentUser.Id, assignedUser.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetCardsAssignedToUserAsync_FiltersCardsByViewerPermission()
    {
        // Arrange
        var currentUser = TestDataFactory.CreateTestUser(email: "current@example.com");
        var otherUser = TestDataFactory.CreateTestUser(email: "other@example.com");
        var assignedUser = TestDataFactory.CreateTestUser(email: "assigned@example.com");
        _context.Users.AddRange(currentUser, otherUser, assignedUser);
        await _context.SaveChangesAsync();

        // Board owned by currentUser
        var board1 = TestDataFactory.CreateTestBoard(currentUser.Id);
        // Board owned by otherUser where currentUser is not a member
        var board2 = TestDataFactory.CreateTestBoard(otherUser.Id);
        _context.Boards.AddRange(board1, board2);
        await _context.SaveChangesAsync();

        var currentMember1 = TestDataFactory.CreateTestBoardMember(board1.Id, currentUser.Id, BoardMemberRole.Viewer);
        var assignedMember1 = TestDataFactory.CreateTestBoardMember(board1.Id, assignedUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(currentMember1, assignedMember1);
        await _context.SaveChangesAsync();

        var column1 = TestDataFactory.CreateTestColumn(board1.Id, "To Do");
        var column2 = TestDataFactory.CreateTestColumn(board2.Id, "To Do");
        _context.Columns.AddRange(column1, column2);
        await _context.SaveChangesAsync();

        var card1 = TestDataFactory.CreateTestCard(column1.Id, "Card 1 (Can See)", createdById: currentUser.Id);
        var card2 = TestDataFactory.CreateTestCard(column2.Id, "Card 2 (Cannot See)", createdById: otherUser.Id);
        _context.Cards.AddRange(card1, card2);
        await _context.SaveChangesAsync();

        var assignment1 = new CardAssignment { CardId = card1.Id, UserId = assignedUser.Id, AssignedAt = DateTime.UtcNow };
        var assignment2 = new CardAssignment { CardId = card2.Id, UserId = assignedUser.Id, AssignedAt = DateTime.UtcNow };
        _context.CardAssignments.AddRange(assignment1, assignment2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _assignmentService.GetCardsAssignedToUserAsync(board1.Id, currentUser.Id, assignedUser.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal(card1.Id, result.Value[0].Id);
    }

    #endregion

    #region Multiple Assignments Tests

    [Fact]
    public async Task MultipleAssignments_CreateAndRetrieve_MaintainsIntegrity()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
        var user1 = TestDataFactory.CreateTestUser(email: "user1@example.com");
        var user2 = TestDataFactory.CreateTestUser(email: "user2@example.com");
        _context.Users.AddRange(owner, user1, user2);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var member1 = TestDataFactory.CreateTestBoardMember(board.Id, user1.Id, BoardMemberRole.Member);
        var member2 = TestDataFactory.CreateTestBoardMember(board.Id, user2.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(ownerMember, member1, member2);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: owner.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        // Act - Assign to multiple users
        var assign1 = await _assignmentService.AssignCardToUserAsync(card.Id, owner.Id, user1.Id);
        var assign2 = await _assignmentService.AssignCardToUserAsync(card.Id, owner.Id, user2.Id);

        // Assert both assignments succeeded
        Assert.True(assign1.IsSuccess);
        Assert.True(assign2.IsSuccess);

        // Retrieve assigned users
        var getResult = await _cardService.GetCardByIdAsync(card.Id, owner.Id);
        Assert.True(getResult.IsSuccess);
        Assert.Equal(2, getResult.Value!.CardAssignments.Count);
        Assert.Contains(getResult.Value.CardAssignments, u => u.UserId == user1.Id);
        Assert.Contains(getResult.Value.CardAssignments, u => u.UserId == user2.Id);

        // Unassign one user
        var unassign = await _assignmentService.UnassignCardFromUserAsync(card.Id, owner.Id, user1.Id);
        Assert.True(unassign.IsSuccess);

        // Verify only one remains
        var finalResult = await _cardService.GetCardByIdAsync(card.Id, owner.Id);
        Assert.Single(finalResult.Value!.CardAssignments);
        Assert.Equal(user2.Id, finalResult.Value!.CardAssignments[0].UserId);
    }

    #endregion
}
