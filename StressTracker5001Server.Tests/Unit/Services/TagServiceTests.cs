using Xunit;
using Microsoft.Extensions.Configuration;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class TagServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BoardAuthorizationService _authService;
    private readonly TagService _tagService;
    private readonly IConfiguration _configuration;

    public TagServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _authService = new BoardAuthorizationService(_context);

        // Create in-memory configuration
        var configData = new Dictionary<string, string?>
    {
      {"Tags:MaxTagsPerBoard", "20"}
    };
        _configuration = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

        _tagService = new TagService(_context, _configuration, _authService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetTagsByBoardIdAsync_ReturnsAllTags()
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

        var tag1 = TestDataFactory.CreateTestTag(board.Id, "Tag 1", "#FF5733");
        var tag2 = TestDataFactory.CreateTestTag(board.Id, "Tag 2", "#33FF57");
        _context.Tags.AddRange(tag1, tag2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.GetTagsByBoardIdAsync(board.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task CreateTagAsync_WithValidData_CreatesTag()
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

        var createDto = new TagCreateDto
        {
            BoardId = board.Id,
            Name = "New Tag",
            Color = "#FF5733"
        };

        // Act
        var result = await _tagService.CreateTagAsync(createDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("New Tag", result.Value.Name);
        Assert.Equal("#FF5733", result.Value.Color);
    }

    [Fact]
    public async Task CreateTagAsync_WithDuplicateName_ReturnsFailure()
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

        var existingTag = TestDataFactory.CreateTestTag(board.Id, "Duplicate Tag");
        _context.Tags.Add(existingTag);
        await _context.SaveChangesAsync();

        var createDto = new TagCreateDto
        {
            BoardId = board.Id,
            Name = "Duplicate Tag",
            Color = "#FF5733"
        };

        // Act
        var result = await _tagService.CreateTagAsync(createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("already exists", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task UpdateTagAsync_WithValidData_UpdatesTag()
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

        var tag = TestDataFactory.CreateTestTag(board.Id, "Old Name");
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var updateDto = new TagUpdateDto
        {
            Name = "Updated Name",
            Color = "#33FF57"
        };

        // Act
        var result = await _tagService.UpdateTagAsync(tag.Id, updateDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated Name", result.Value.Name);
        Assert.Equal("#33FF57", result.Value.Color);
    }

    [Fact]
    public async Task DeleteTagAsync_WithValidTag_DeletesTag()
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

        var tag = TestDataFactory.CreateTestTag(board.Id, "To Delete");
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.DeleteTagAsync(tag.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        // Verify tag was deleted
        var deletedTag = await _context.Tags.FindAsync(tag.Id);
        Assert.Null(deletedTag);
    }

    [Fact]
    public async Task GetTagsByBoardIdAsync_WithoutPermission_ReturnsForbidden()
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
        var result = await _tagService.GetTagsByBoardIdAsync(board.Id, nonMember.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }
}
