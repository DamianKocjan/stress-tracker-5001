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

        // Owner needs to be a board member with Admin role to add members
        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AddMemberAsync(board.Id, newMember.Id, owner.Id, BoardMemberRole.Member);

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

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AddMemberAsync(board.Id, member.Id, owner.Id, BoardMemberRole.Member);

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
        var newMember = TestDataFactory.CreateTestUser(email: "newmember@example.com");
        _context.Users.AddRange(user, newMember);
        await _context.SaveChangesAsync();

        var nonExistentBoardId = 999;

        // Act
        var result = await _authService.AddMemberAsync(nonExistentBoardId, newMember.Id, user.Id, BoardMemberRole.Member);

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

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.RemoveMemberAsync(board.Id, member.Id, owner.Id);

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

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember1 = TestDataFactory.CreateTestBoardMember(board.Id, member1.Id, BoardMemberRole.Member);
        var boardMember2 = TestDataFactory.CreateTestBoardMember(board.Id, member2.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(ownerMember, boardMember1, boardMember2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetMembersAsync(board.Id, owner.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count);
    }

    [Fact]
    public async Task GetBoardUserRoleByIdAsync_AsOwner_ReturnsOwnerRole()
    {
        // Arrange
        // Owner is now automatically added as a BoardMember with Owner role
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        // Create owner member (simulates what CreateBoardAsync does in the real service)
        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Owner);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetBoardUserRoleByIdAsync(board.Id, user.Id);

        // Assert
        // Owner is now a member with Owner role
        Assert.True(result.IsSuccess);
        Assert.Equal(BoardMemberRole.Owner, result.Value);
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

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AddMemberAsync(board.Id, member.Id, owner.Id, role);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(role, result.Value.Role);
    }

    #region Permission Tests - AddMember

    [Fact]
    public async Task AddMemberAsync_AsMember_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var memberUser = TestDataFactory.CreateTestMemberUser();
        var newMember = TestDataFactory.CreateTestUser(email: "newmember@example.com");
        _context.Users.AddRange(owner, memberUser, newMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, memberUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AddMemberAsync(board.Id, newMember.Id, memberUser.Id, BoardMemberRole.Member);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task AddMemberAsync_AsViewer_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var viewerUser = TestDataFactory.CreateTestViewerUser();
        var newMember = TestDataFactory.CreateTestUser(email: "newmember@example.com");
        _context.Users.AddRange(owner, viewerUser, newMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, viewerUser.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AddMemberAsync(board.Id, newMember.Id, viewerUser.Id, BoardMemberRole.Member);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task AddMemberAsync_AsNonMember_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var nonMember = TestDataFactory.CreateTestNonMemberUser();
        var newMember = TestDataFactory.CreateTestUser(email: "newmember@example.com");
        _context.Users.AddRange(owner, nonMember, newMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AddMemberAsync(board.Id, newMember.Id, nonMember.Id, BoardMemberRole.Member);

        // Assert
        result.AssertForbidden("permission");
    }

    #endregion

    #region Permission Tests - RemoveMember

    [Fact]
    public async Task RemoveMemberAsync_AsMember_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var memberUser = TestDataFactory.CreateTestMemberUser();
        var targetMember = TestDataFactory.CreateTestUser(email: "target@example.com");
        _context.Users.AddRange(owner, memberUser, targetMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, memberUser.Id, BoardMemberRole.Member);
        var targetBoardMember = TestDataFactory.CreateTestBoardMember(board.Id, targetMember.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(ownerMember, boardMember, targetBoardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.RemoveMemberAsync(board.Id, targetMember.Id, memberUser.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task RemoveMemberAsync_AsViewer_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var viewerUser = TestDataFactory.CreateTestViewerUser();
        var targetMember = TestDataFactory.CreateTestUser(email: "target@example.com");
        _context.Users.AddRange(owner, viewerUser, targetMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, viewerUser.Id, BoardMemberRole.Viewer);
        var targetBoardMember = TestDataFactory.CreateTestBoardMember(board.Id, targetMember.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(ownerMember, boardMember, targetBoardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.RemoveMemberAsync(board.Id, targetMember.Id, viewerUser.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task RemoveMemberAsync_NonExistentMember_ReturnsNotFound()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        _context.Users.Add(owner);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        var nonExistentMemberId = 999;

        // Act
        var result = await _authService.RemoveMemberAsync(board.Id, nonExistentMemberId, owner.Id);

        // Assert
        result.AssertNotFound();
    }

    #endregion

    #region Permission Tests - GetMembers

    [Fact]
    public async Task GetMembersAsync_AsMember_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var memberUser = TestDataFactory.CreateTestMemberUser();
        _context.Users.AddRange(owner, memberUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, memberUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetMembersAsync(board.Id, memberUser.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task GetMembersAsync_AsViewer_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var viewerUser = TestDataFactory.CreateTestViewerUser();
        _context.Users.AddRange(owner, viewerUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, viewerUser.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetMembersAsync(board.Id, viewerUser.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task GetMembersAsync_NonExistentBoard_ReturnsNotFound()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        _context.Users.Add(owner);
        await _context.SaveChangesAsync();

        var nonExistentBoardId = 999;

        // Act
        var result = await _authService.GetMembersAsync(nonExistentBoardId, owner.Id);

        // Assert
        result.AssertNotFound();
    }

    #endregion

    #region ChangeMemberRole Tests

    [Fact]
    public async Task ChangeMemberRoleAsync_FromViewerToMember_Succeeds()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var memberUser = TestDataFactory.CreateTestViewerUser();
        _context.Users.AddRange(owner, memberUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, memberUser.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.ChangeMemberRoleAsync(board.Id, owner.Id, memberUser.Id, BoardMemberRole.Member);

        // Assert
        var updatedMember = result.AssertSuccess();
        Assert.Equal(BoardMemberRole.Member, updatedMember.Role);
    }

    [Fact]
    public async Task ChangeMemberRoleAsync_FromMemberToAdmin_Succeeds()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var memberUser = TestDataFactory.CreateTestMemberUser();
        _context.Users.AddRange(owner, memberUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, memberUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.ChangeMemberRoleAsync(board.Id, owner.Id, memberUser.Id, BoardMemberRole.Admin);

        // Assert
        var updatedMember = result.AssertSuccess();
        Assert.Equal(BoardMemberRole.Admin, updatedMember.Role);
    }

    [Fact]
    public async Task ChangeMemberRoleAsync_FromAdminToViewer_Succeeds()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var adminUser = TestDataFactory.CreateTestAdminUser("admin2@test.com");
        _context.Users.AddRange(owner, adminUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, adminUser.Id, BoardMemberRole.Admin);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.ChangeMemberRoleAsync(board.Id, owner.Id, adminUser.Id, BoardMemberRole.Viewer);

        // Assert
        var updatedMember = result.AssertSuccess();
        Assert.Equal(BoardMemberRole.Viewer, updatedMember.Role);
    }

    [Fact]
    public async Task ChangeMemberRoleAsync_AsMember_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var memberUser = TestDataFactory.CreateTestMemberUser();
        var targetUser = TestDataFactory.CreateTestViewerUser();
        _context.Users.AddRange(owner, memberUser, targetUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, memberUser.Id, BoardMemberRole.Member);
        var targetMember = TestDataFactory.CreateTestBoardMember(board.Id, targetUser.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(ownerMember, boardMember, targetMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.ChangeMemberRoleAsync(board.Id, memberUser.Id, targetUser.Id, BoardMemberRole.Member);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task ChangeMemberRoleAsync_NonExistentMember_ReturnsNotFound()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        _context.Users.Add(owner);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        var nonExistentMemberId = 999;

        // Act
        var result = await _authService.ChangeMemberRoleAsync(board.Id, owner.Id, nonExistentMemberId, BoardMemberRole.Member);

        // Assert
        result.AssertNotFound();
    }

    [Theory]
    [InlineData(BoardMemberRole.Viewer)]
    [InlineData(BoardMemberRole.Member)]
    [InlineData(BoardMemberRole.Admin)]
    [InlineData(BoardMemberRole.Owner)]
    public async Task ChangeMemberRoleAsync_ToAnyRole_Succeeds(BoardMemberRole targetRole)
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var memberUser = TestDataFactory.CreateTestMemberUser();
        _context.Users.AddRange(owner, memberUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, memberUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.ChangeMemberRoleAsync(board.Id, owner.Id, memberUser.Id, targetRole);

        // Assert
        var updatedMember = result.AssertSuccess();
        Assert.Equal(targetRole, updatedMember.Role);
    }

    #endregion

    #region UserCanAccessBoardAsync Tests

    [Theory]
    [InlineData(BoardMemberRole.Admin, BoardMemberRole.Admin, true)]
    [InlineData(BoardMemberRole.Admin, BoardMemberRole.Member, true)]
    [InlineData(BoardMemberRole.Admin, BoardMemberRole.Viewer, true)]
    [InlineData(BoardMemberRole.Member, BoardMemberRole.Admin, false)]
    [InlineData(BoardMemberRole.Member, BoardMemberRole.Member, true)]
    [InlineData(BoardMemberRole.Member, BoardMemberRole.Viewer, true)]
    [InlineData(BoardMemberRole.Viewer, BoardMemberRole.Admin, false)]
    [InlineData(BoardMemberRole.Viewer, BoardMemberRole.Member, false)]
    [InlineData(BoardMemberRole.Viewer, BoardMemberRole.Viewer, true)]
    public async Task UserCanAccessBoardAsync_WithDifferentRoleRequirements_ReturnsCorrect(
        BoardMemberRole userRole,
        BoardMemberRole requiredRole,
        bool expectedAccess)
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, userRole);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.UserCanAccessBoardAsync(board.Id, user.Id, requiredRole);

        // Assert
        Assert.Equal(expectedAccess, result);
    }

    [Fact]
    public async Task UserCanAccessBoardAsync_NonMember_ReturnsFalse()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var nonMember = TestDataFactory.CreateTestNonMemberUser();
        _context.Users.AddRange(owner, nonMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.UserCanAccessBoardAsync(board.Id, nonMember.Id, BoardMemberRole.Viewer);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserCanAccessBoardAsync_DefaultRoleViewer_ReturnsTrueForAllRoles()
    {
        // Arrange
        var user = TestDataFactory.CreateTestAdminUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        // Act - without specifying requiredRole (defaults to Viewer)
        var result = await _authService.UserCanAccessBoardAsync(board.Id, user.Id);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task AddMemberAsync_WithOwnerRole_Succeeds()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var newMember = TestDataFactory.CreateTestUser(email: "newowner@example.com");
        _context.Users.AddRange(owner, newMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AddMemberAsync(board.Id, newMember.Id, owner.Id, BoardMemberRole.Owner);

        // Assert
        var member = result.AssertSuccess();
        Assert.Equal(BoardMemberRole.Owner, member.Role);
    }

    [Fact]
    public async Task GetMemberRoleAsync_WithValidMember_ReturnsRole()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        var member = TestDataFactory.CreateTestMemberUser();
        _context.Users.AddRange(owner, member);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(ownerMember, boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetMemberRoleAsync(board.Id, member.Id);

        // Assert
        var returnedMember = result.AssertSuccess();
        Assert.Equal(BoardMemberRole.Member, returnedMember.Role);
    }

    [Fact]
    public async Task GetMemberRoleAsync_NonExistentMember_ReturnsNotFound()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        _context.Users.Add(owner);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        var nonExistentMemberId = 999;

        // Act
        var result = await _authService.GetMemberRoleAsync(board.Id, nonExistentMemberId);

        // Assert
        result.AssertNotFound();
    }

    [Fact]
    public async Task GetMembersAsync_EmptyBoard_ReturnsEmptyList()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestAdminUser();
        _context.Users.Add(owner);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(ownerMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetMembersAsync(board.Id, owner.Id);

        // Assert
        var members = result.AssertSuccess();
        Assert.Single(members); // Only the owner
    }

    [Fact]
    public async Task GetMembersAsync_WithMultipleRoles_ReturnsAllMembers()
    {
        // Arrange
        var admin = TestDataFactory.CreateTestAdminUser();
        var member = TestDataFactory.CreateTestMemberUser();
        var viewer = TestDataFactory.CreateTestViewerUser();
        _context.Users.AddRange(admin, member, viewer);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        var memberMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id, BoardMemberRole.Member);
        var viewerMember = TestDataFactory.CreateTestBoardMember(board.Id, viewer.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(adminMember, memberMember, viewerMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetMembersAsync(board.Id, admin.Id);

        // Assert
        var members = result.AssertSuccess();
        Assert.Equal(3, members.Count);
        Assert.Single(members, m => m.Role == BoardMemberRole.Admin);
        Assert.Single(members, m => m.Role == BoardMemberRole.Member);
        Assert.Single(members, m => m.Role == BoardMemberRole.Viewer);
    }

    #endregion
}
