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

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
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

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
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
}
