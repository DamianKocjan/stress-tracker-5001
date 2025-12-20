using Xunit;
using Moq;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.Tests.Helpers;
using Microsoft.Extensions.Configuration;

namespace StressTracker5001Server.Tests.Integration;

/// <summary>
/// Integration tests verify multiple components working together
/// These tests use real service implementations with InMemory database
/// </summary>
public class BoardWorkflowIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BoardService _boardService;
    private readonly BoardAuthorizationService _authService;
    private readonly UserService _userService;
    private readonly Mock<IActivityLogService> _mockActivityLogService;
    private readonly IConfiguration _configuration;

    public BoardWorkflowIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mockActivityLogService = MockServiceFactory.CreateMockActivityLogService();

        // Create in-memory configuration
        var configData = new Dictionary<string, string?>
        {
            {"Auth:TokenChars", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz"},
            {"Auth:TokenLength", "32"},
            {"Auth:PasswordReset:TokenExpiryMinutes", "60"},
            {"Auth:EmailVerification:TokenExpiryMinutes", "1440"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _authService = new BoardAuthorizationService(_context, _mockActivityLogService.Object);
        _boardService = new BoardService(_context, _authService, _mockActivityLogService.Object);
        _userService = new UserService(_context, _configuration);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CompleteBoardWorkflow_CreateBoardAndAddMembers_Success()
    {
        // Arrange - Create users
        var ownerDto = new DTOs.User.CreateUserDto
        {
            Email = "owner@example.com",
            Username = "owner",
            Password = "Password123"
        };
        var memberDto = new DTOs.User.CreateUserDto
        {
            Email = "member@example.com",
            Username = "member",
            Password = "Password123"
        };

        var ownerResult = await _userService.CreateUserAsync(ownerDto);
        var memberResult = await _userService.CreateUserAsync(memberDto);

        Assert.True(ownerResult.IsSuccess);
        Assert.True(memberResult.IsSuccess);

        var ownerId = ownerResult.Value!.Id;
        var memberId = memberResult.Value!.Id;

        // Act 1 - Owner creates a board
        var createBoardDto = new CreateBoardDto
        {
            Name = "Project Board",
            Description = "Team collaboration board"
        };

        var createBoardResult = await _boardService.CreateBoardAsync(createBoardDto, ownerId);
        Assert.True(createBoardResult.IsSuccess);

        var board = createBoardResult.Value;
        var boardId = board!.Id;

        // Verify board creation activity was logged
        _mockActivityLogService.Verify(
            s => s.LogBoardCreatedAsync(boardId, ownerId, createBoardDto.Name),
            Times.Once);

        // Act 2 - Owner adds a member to the board
        // Owner is automatically created as a member with Owner role (which has Admin permissions)
        var addMemberResult = await _authService.AddMemberAsync(boardId, memberId, ownerId, BoardMemberRole.Member);
        Assert.True(addMemberResult.IsSuccess);

        // Act 3 - Verify member can access the board
        var memberAccessResult = await _boardService.GetBoardByIdAsync(boardId, memberId);
        Assert.True(memberAccessResult.IsSuccess);

        // Act 4 - Verify member has correct role
        var memberRoleResult = await _authService.GetBoardUserRoleByIdAsync(boardId, memberId);
        Assert.True(memberRoleResult.IsSuccess);
        Assert.Equal(BoardMemberRole.Member, memberRoleResult.Value);

        // Act 5 - Verify board owner can see the member
        var membersResult = await _authService.GetMembersAsync(boardId, ownerId);
        Assert.True(membersResult.IsSuccess);
        Assert.Equal(2, membersResult.Value!.Count); // Owner (with Owner role) and Member

        // Verify owner role is Owner (not Admin)
        var ownerRole = membersResult.Value.FirstOrDefault(m => m.UserId == ownerId);
        Assert.NotNull(ownerRole);
        Assert.Equal(BoardMemberRole.Owner, ownerRole.Role);

        // Act 6 - Update board details
        var updateDto = new UpdateBoardDto
        {
            Name = "Updated Project Board",
            Description = "Updated description"
        };

        var updateResult = await _boardService.UpdateBoardAsync(boardId, updateDto, ownerId);
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(updateDto.Name, updateResult.Value!.Name);

        // Verify board update activity was logged with diff
        _mockActivityLogService.Verify(
            s => s.LogBoardUpdatedAsync(boardId, ownerId, It.IsAny<object>(), It.IsAny<object>()),
            Times.Once);

        // Act 7 - Remove member
        var removeResult = await _authService.RemoveMemberAsync(boardId, memberId, ownerId);
        Assert.True(removeResult.IsSuccess);

        // Act 8 - Verify member no longer has access
        var noAccessResult = await _boardService.GetBoardByIdAsync(boardId, memberId);
        Assert.False(noAccessResult.IsSuccess);
        Assert.Equal(403, noAccessResult.StatusCode);

        // Act 9 - Delete the board
        var deleteResult = await _boardService.DeleteBoardAsync(boardId, ownerId);
        Assert.True(deleteResult.IsSuccess);

        // Verify board deletion activity was logged
        _mockActivityLogService.Verify(
            s => s.LogBoardDeletedAsync(boardId, ownerId, It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task BoardAuthorization_NonMemberCannotAccessBoard()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
        var nonMember = TestDataFactory.CreateTestUser(email: "nonmember@example.com");

        _context.Users.AddRange(owner, nonMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        // Act - Non-member tries to access board
        var accessResult = await _boardService.GetBoardByIdAsync(board.Id, nonMember.Id);

        // Assert
        Assert.False(accessResult.IsSuccess);
        Assert.Equal(403, accessResult.StatusCode);
    }

    [Fact]
    public async Task BoardDeletion_RemovesAllRelatedData()
    {
        // Arrange - Create a board with members
        var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
        var member = TestDataFactory.CreateTestUser(email: "member@example.com");

        _context.Users.AddRange(owner, member);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        // Create owner and member as board members
        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Owner);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        var boardColumn = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(boardColumn);
        await _context.SaveChangesAsync();

        var boardCard = TestDataFactory.CreateTestCard(boardColumn.Id, "Test Card");
        _context.Cards.Add(boardCard);
        await _context.SaveChangesAsync();

        var boardCardComment = TestDataFactory.CreateTestComment(boardCard.Id, member.Id, "This is a comment.");
        _context.Comments.Add(boardCardComment);
        await _context.SaveChangesAsync();

        // Act - Delete the board
        var deleteResult = await _boardService.DeleteBoardAsync(board.Id, owner.Id);

        // Assert
        Assert.True(deleteResult.IsSuccess);

        // Verify board is deleted
        var boardExists = await _context.Boards.FindAsync(board.Id);
        Assert.Null(boardExists);

        // Verify related members are deleted (cascade)
        var memberExists = await _context.BoardMembers.FindAsync(boardMember.Id);
        Assert.Null(memberExists);

        // Verify related columns are deleted (cascade)
        var columnExists = await _context.Columns.FindAsync(boardColumn.Id);
        Assert.Null(columnExists);

        // Verify related cards are deleted (cascade)
        var cardExists = await _context.Cards.FindAsync(boardCard.Id);
        Assert.Null(cardExists);

        // Verify related comments are deleted (cascade)
        var commentExists = await _context.Comments.FindAsync(boardCardComment.Id);
        Assert.Null(commentExists);
    }
}
