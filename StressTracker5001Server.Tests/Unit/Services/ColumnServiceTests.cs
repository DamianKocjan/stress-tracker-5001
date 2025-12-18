using Xunit;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class ColumnServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BoardAuthorizationService _authService;
    private readonly ColumnService _columnService;

    public ColumnServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _authService = new BoardAuthorizationService(_context);
        _columnService = new ColumnService(_context, _authService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetColumnByIdAsync_WithValidColumn_ReturnsColumn()
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

        // Act
        var result = await _columnService.GetColumnByIdAsync(column.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(column.Id, result.Value.Id);
        Assert.Equal("To Do", result.Value.Name);
    }

    [Fact]
    public async Task CreateColumnAsync_WithValidData_CreatesColumn()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var createDto = new CreateColumnDto
        {
            Name = "New Column",
            Position = 0,
            WipLimit = 5
        };

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("New Column", result.Value.Name);
        Assert.Equal(5, result.Value.WipLimit);
    }

    [Fact]
    public async Task UpdateColumnAsync_WithValidData_UpdatesColumn()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "Old Name");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateColumnDto
        {
            Name = "Updated Name",
            WipLimit = 10
        };

        // Act
        var result = await _columnService.UpdateColumnAsync(column.Id, updateDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated Name", result.Value.Name);
        Assert.Equal(10, result.Value.WipLimit);
    }

    [Fact]
    public async Task DeleteColumnAsync_WithValidColumn_DeletesColumn()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Delete");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        // Act
        var result = await _columnService.DeleteColumnAsync(column.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        // Verify column was deleted
        var deletedColumn = await _context.Columns.FindAsync(column.Id);
        Assert.Null(deletedColumn);
    }

    [Fact]
    public async Task GetColumnByIdAsync_WithoutPermission_ReturnsForbidden()
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

        // Act
        var result = await _columnService.GetColumnByIdAsync(column.Id, nonMember.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task GetColumnByIdAsync_WhenColumnNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _columnService.GetColumnByIdAsync(columnId: 9999, userId: user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task CreateColumnAsync_WhenBoardNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var createDto = new CreateColumnDto
        {
            Name = "New Column",
            Position = 0,
            WipLimit = 5
        };

        // Act
        var result = await _columnService.CreateColumnAsync(boardId: 9999, createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task CreateColumnAsync_WithMemberRole_ReturnsForbidden()
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

        var createDto = new CreateColumnDto
        {
            Name = "New Column",
            Position = 0,
            WipLimit = 5
        };

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task CreateColumnAsync_WithInvalidPosition_ReturnsFailure()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var createDto = new CreateColumnDto
        {
            Name = "New Column",
            Position = -1,
            WipLimit = 5
        };

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task UpdateColumnAsync_WithViewerRole_ReturnsForbidden()
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

        var column = TestDataFactory.CreateTestColumn(board.Id, "Old Name");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateColumnDto
        {
            Name = "Updated Name",
            WipLimit = 10
        };

        // Act
        var result = await _columnService.UpdateColumnAsync(column.Id, updateDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task UpdateColumnAsync_WithNegativeWipLimit_ReturnsFailure()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "Old Name");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateColumnDto
        {
            Name = "Updated Name",
            WipLimit = -10
        };

        // Act
        var result = await _columnService.UpdateColumnAsync(column.Id, updateDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task DeleteColumnAsync_WithMemberRole_ReturnsForbidden()
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

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Delete");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        // Act
        var result = await _columnService.DeleteColumnAsync(column.Id, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task MoveColumnAsync_WithOutOfRangePosition_ReturnsFailure()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        var column1 = TestDataFactory.CreateTestColumn(board.Id, "Col 1", position: 0);
        var column2 = TestDataFactory.CreateTestColumn(board.Id, "Col 2", position: 1);
        _context.Columns.AddRange(column1, column2);
        await _context.SaveChangesAsync();

        // Act (valid indices are 0..1)
        var result = await _columnService.MoveColumnAsync(column1.Id, newPosition: 2, userId: user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    #region Input Validation Tests

    [Fact]
    public async Task CreateColumnAsync_WithNullName_ReturnsBadRequest()
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

        var createDto = new CreateColumnDto
        {
            Name = null!,  // Null name
            Position = 0,
            WipLimit = 5
        };

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, user.Id);

        // Assert
        result.AssertBadRequest("required");
    }

    [Fact]
    public async Task CreateColumnAsync_WithEmptyName_ReturnsBadRequest()
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

        var createDto = new CreateColumnDto
        {
            Name = "",
            Position = 0,
            WipLimit = 5
        };

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, user.Id);

        // Assert
        result.AssertBadRequest("required");
    }

    [Fact]
    public async Task CreateColumnAsync_WithWhitespaceName_ReturnsBadRequest()
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

        var createDto = new CreateColumnDto
        {
            Name = "   ",
            Position = 0,
            WipLimit = 5
        };

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, user.Id);

        // Assert
        result.AssertBadRequest("required");
    }

    [Fact]
    public async Task CreateColumnAsync_WithNegativeWipLimit_ReturnsBadRequest()
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

        var createDto = new CreateColumnDto
        {
            Name = "New Column",
            Position = 0,
            WipLimit = -5
        };

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, user.Id);

        // Assert
        result.AssertBadRequest("greater than");
    }

    [Fact]
    public async Task CreateColumnAsync_WithZeroWipLimit_Succeeds()
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

        var createDto = new CreateColumnDto
        {
            Name = "New Column",
            Position = 0,
            WipLimit = 0
        };

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, user.Id);

        // Assert
        var column = result.AssertSuccess();
        Assert.Equal(0, column.WipLimit);
    }

    [Fact]
    public async Task CreateColumnAsync_WithLargeWipLimit_Succeeds()
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

        var createDto = new CreateColumnDto
        {
            Name = "New Column",
            Position = 0,
            WipLimit = 1000
        };

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, user.Id);

        // Assert
        var column = result.AssertSuccess();
        Assert.Equal(1000, column.WipLimit);
    }

    [Fact]
    public async Task UpdateColumnAsync_WithEmptyName_ReturnsBadRequest()
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

        var column = TestDataFactory.CreateTestColumn(board.Id, "Old Name");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateColumnDto
        {
            Name = "",
            WipLimit = 10
        };

        // Act
        var result = await _columnService.UpdateColumnAsync(column.Id, updateDto, user.Id);

        // Assert
        result.AssertBadRequest("required");
    }

    #endregion

    #region Permission Tests

    [Fact]
    public async Task CreateColumnAsync_AsViewer_ReturnsForbidden()
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

        var createDto = TestDataFactory.CreateColumnDto();

        // Act
        var result = await _columnService.CreateColumnAsync(board.Id, createDto, viewer.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task UpdateColumnAsync_AsMember_ReturnsForbidden()
    {
        // Arrange
        var admin = TestDataFactory.CreateTestAdminUser();
        var member = TestDataFactory.CreateTestMemberUser();
        _context.Users.AddRange(admin, member);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        var memberMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(adminMember, memberMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "Test Column");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateColumnDto { Name = "Updated", WipLimit = 5 };

        // Act
        var result = await _columnService.UpdateColumnAsync(column.Id, updateDto, member.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task DeleteColumnAsync_AsViewer_ReturnsForbidden()
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

        var column = TestDataFactory.CreateTestColumn(board.Id, "Test Column");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        // Act
        var result = await _columnService.DeleteColumnAsync(column.Id, viewer.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    [Fact]
    public async Task MoveColumnAsync_AsNonMember_ReturnsForbidden()
    {
        // Arrange
        var admin = TestDataFactory.CreateTestAdminUser();
        var nonMember = TestDataFactory.CreateTestNonMemberUser();
        _context.Users.AddRange(admin, nonMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(adminMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "Test Column");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        // Act
        var result = await _columnService.MoveColumnAsync(column.Id, 0, nonMember.Id);

        // Assert
        result.AssertForbidden("permission");
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public async Task MoveColumnAsync_WithMultipleColumns_ReordersCorrectly()
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

        // Create columns: 0, 1, 2
        var col1 = TestDataFactory.CreateTestColumn(board.Id, "Col 1", position: 0);
        var col2 = TestDataFactory.CreateTestColumn(board.Id, "Col 2", position: 1);
        var col3 = TestDataFactory.CreateTestColumn(board.Id, "Col 3", position: 2);
        _context.Columns.AddRange(col1, col2, col3);
        await _context.SaveChangesAsync();

        // Act: Move col1 to position 2
        var result = await _columnService.MoveColumnAsync(col1.Id, 2, user.Id);

        // Assert
        result.AssertSuccess();

        // Reload columns
        var updatedCol1 = await _context.Columns.FindAsync(col1.Id);
        var updatedCol2 = await _context.Columns.FindAsync(col2.Id);
        var updatedCol3 = await _context.Columns.FindAsync(col3.Id);

        // Now col2, col3, col1 should be at 0, 1, 2
        Assert.Equal(0, updatedCol2!.Position);
        Assert.Equal(1, updatedCol3!.Position);
        Assert.Equal(2, updatedCol1!.Position);
    }

    [Fact]
    public async Task MoveColumnAsync_ToFirstPosition_Succeeds()
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

        var col1 = TestDataFactory.CreateTestColumn(board.Id, "Col 1", position: 0);
        var col2 = TestDataFactory.CreateTestColumn(board.Id, "Col 2", position: 1);
        _context.Columns.AddRange(col1, col2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _columnService.MoveColumnAsync(col2.Id, 0, user.Id);

        // Assert
        var movedColumn = result.AssertSuccess();
        Assert.Equal(0, movedColumn.Position);
    }

    [Fact]
    public async Task MoveColumnAsync_ToLastPosition_Succeeds()
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

        var col1 = TestDataFactory.CreateTestColumn(board.Id, "Col 1", position: 0);
        var col2 = TestDataFactory.CreateTestColumn(board.Id, "Col 2", position: 1);
        var col3 = TestDataFactory.CreateTestColumn(board.Id, "Col 3", position: 2);
        _context.Columns.AddRange(col1, col2, col3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _columnService.MoveColumnAsync(col1.Id, 2, user.Id);

        // Assert
        var movedColumn = result.AssertSuccess();
        Assert.Equal(2, movedColumn.Position);
    }

    [Fact]
    public async Task MoveColumnAsync_ToSamePosition_Succeeds()
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

        var column = TestDataFactory.CreateTestColumn(board.Id, "Col", position: 0);
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        // Act
        var result = await _columnService.MoveColumnAsync(column.Id, 0, user.Id);

        // Assert
        result.AssertSuccess();
    }

    [Fact]
    public async Task MoveColumnAsync_WithNegativePosition_ReturnsBadRequest()
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

        var column = TestDataFactory.CreateTestColumn(board.Id, "Col", position: 0);
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        // Act
        var result = await _columnService.MoveColumnAsync(column.Id, -1, user.Id);

        // Assert
        result.AssertBadRequest("greater than");
    }

    [Fact]
    public async Task GetColumnByIdAsync_WithDifferentRequiredRoles_ReturnsCorrectly()
    {
        // Arrange
        var viewer = TestDataFactory.CreateTestViewerUser();
        var member = TestDataFactory.CreateTestMemberUser();
        _context.Users.AddRange(viewer, member);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(viewer.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var viewerMember = TestDataFactory.CreateTestBoardMember(board.Id, viewer.Id, BoardMemberRole.Viewer);
        var memberMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(viewerMember, memberMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "Col");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        // Act & Assert - Viewer can access with Viewer requirement
        var result1 = await _columnService.GetColumnByIdAsync(column.Id, viewer.Id, BoardMemberRole.Viewer);
        result1.AssertSuccess();

        // Member can access with Viewer requirement (higher role)
        var result2 = await _columnService.GetColumnByIdAsync(column.Id, member.Id, BoardMemberRole.Viewer);
        result2.AssertSuccess();

        // Viewer cannot access with Member requirement (lower role)
        var result3 = await _columnService.GetColumnByIdAsync(column.Id, viewer.Id, BoardMemberRole.Member);
        result3.AssertForbidden();
    }

    #endregion
}
