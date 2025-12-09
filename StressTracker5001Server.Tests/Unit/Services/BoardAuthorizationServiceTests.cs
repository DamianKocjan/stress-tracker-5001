using Xunit;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class BoardAuthorizationServiceTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly BoardAuthorizationService _authService;

  public BoardAuthorizationServiceTests()
  {
    _context = TestDbContextFactory.CreateInMemoryDbContext();
    _authService = new BoardAuthorizationService(_context);
  }

  public void Dispose()
  {
    _context.Database.EnsureDeleted();
    _context.Dispose();
  }

  [Fact]
  public async Task AddMemberAsync_WithValidData_AddsMember()
  {
    // Arrange
    var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
    var newMember = TestDataFactory.CreateTestUser(email: "member@example.com");
    _context.Users.AddRange(owner, newMember);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(owner.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Act
    var result = await _authService.AddMemberAsync(board.Id, newMember.Id, BoardMemberRole.Member);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(board.Id, result.Value.BoardId);
    Assert.Equal(newMember.Id, result.Value.UserId);
    Assert.Equal(BoardMemberRole.Member, result.Value.Role);
  }

  [Fact]
  public async Task AddMemberAsync_WhenAlreadyMember_ReturnsFailure()
  {
    // Arrange
    var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
    var member = TestDataFactory.CreateTestUser(email: "member@example.com");
    _context.Users.AddRange(owner, member);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(owner.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id);
    _context.BoardMembers.Add(boardMember);
    await _context.SaveChangesAsync();

    // Act
    var result = await _authService.AddMemberAsync(board.Id, member.Id, BoardMemberRole.Member);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(400, result.StatusCode);
    Assert.Contains("already a member", result.Error ?? string.Empty);
  }

  [Fact]
  public async Task AddMemberAsync_WithInvalidBoard_ReturnsNotFound()
  {
    // Arrange
    var user = TestDataFactory.CreateTestUser();
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    var nonExistentBoardId = 999;

    // Act
    var result = await _authService.AddMemberAsync(nonExistentBoardId, user.Id, BoardMemberRole.Member);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(404, result.StatusCode);
  }

  [Fact]
  public async Task RemoveMemberAsync_WithValidMember_RemovesMember()
  {
    // Arrange
    var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
    var member = TestDataFactory.CreateTestUser(email: "member@example.com");
    _context.Users.AddRange(owner, member);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(owner.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id);
    _context.BoardMembers.Add(boardMember);
    await _context.SaveChangesAsync();

    // Act
    var result = await _authService.RemoveMemberAsync(board.Id, member.Id);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value);

    // Verify member was removed
    var removedMember = await _context.BoardMembers
        .FindAsync(boardMember.Id);
    Assert.Null(removedMember);
  }

  [Fact]
  public async Task GetMembersAsync_ReturnsAllBoardMembers()
  {
    // Arrange
    var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
    var member1 = TestDataFactory.CreateTestUser(email: "member1@example.com");
    var member2 = TestDataFactory.CreateTestUser(email: "member2@example.com");
    _context.Users.AddRange(owner, member1, member2);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(owner.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    var boardMember1 = TestDataFactory.CreateTestBoardMember(board.Id, member1.Id, BoardMemberRole.Member);
    var boardMember2 = TestDataFactory.CreateTestBoardMember(board.Id, member2.Id, BoardMemberRole.Viewer);
    _context.BoardMembers.AddRange(boardMember1, boardMember2);
    await _context.SaveChangesAsync();

    // Act
    var result = await _authService.GetMembersAsync(board.Id);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.Count);
  }

  [Fact]
  public async Task GetBoardUserRoleByIdAsync_AsOwner_ReturnsNotFound()
  {
    // Arrange
    // Note: The service only checks BoardMembers table, not ownership
    // This test verifies current behavior - owner is not in members table by default
    var user = TestDataFactory.CreateTestUser();
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(user.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Act
    var result = await _authService.GetBoardUserRoleByIdAsync(board.Id, user.Id);

    // Assert
    // Owner is not automatically a member, so returns NotFound
    Assert.False(result.IsSuccess);
    Assert.Equal(404, result.StatusCode);
  }

  [Fact]
  public async Task GetBoardUserRoleByIdAsync_AsMember_ReturnsMemberRole()
  {
    // Arrange
    var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
    var member = TestDataFactory.CreateTestUser(email: "member@example.com");
    _context.Users.AddRange(owner, member);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(owner.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id, BoardMemberRole.Viewer);
    _context.BoardMembers.Add(boardMember);
    await _context.SaveChangesAsync();

    // Act
    var result = await _authService.GetBoardUserRoleByIdAsync(board.Id, member.Id);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(BoardMemberRole.Viewer, result.Value);
  }

  [Fact]
  public async Task IsUserBoardMemberAsync_AsMember_ReturnsTrue()
  {
    // Arrange
    var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
    var member = TestDataFactory.CreateTestUser(email: "member@example.com");
    _context.Users.AddRange(owner, member);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(owner.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id);
    _context.BoardMembers.Add(boardMember);
    await _context.SaveChangesAsync();

    // Act
    var result = await _authService.IsUserBoardMemberAsync(board.Id, member.Id);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value);
  }

  [Fact]
  public async Task IsUserBoardMemberAsync_AsNonMember_ReturnsFalse()
  {
    // Arrange
    var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
    var nonMember = TestDataFactory.CreateTestUser(email: "nonmember@example.com");
    _context.Users.AddRange(owner, nonMember);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(owner.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Act
    var result = await _authService.IsUserBoardMemberAsync(board.Id, nonMember.Id);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.False(result.Value);
  }

  [Theory]
  [InlineData(BoardMemberRole.Admin)]
  [InlineData(BoardMemberRole.Member)]
  [InlineData(BoardMemberRole.Viewer)]
  public async Task AddMemberAsync_WithDifferentRoles_CreatesCorrectRole(BoardMemberRole role)
  {
    // Arrange
    var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
    var member = TestDataFactory.CreateTestUser(email: $"member_{role}@example.com");
    _context.Users.AddRange(owner, member);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(owner.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Act
    var result = await _authService.AddMemberAsync(board.Id, member.Id, role);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(role, result.Value.Role);
  }
}
