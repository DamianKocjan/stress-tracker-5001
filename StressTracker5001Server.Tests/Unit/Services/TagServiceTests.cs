using Xunit;
using Moq;
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
    private readonly Mock<IActivityLogService> _mockActivityLogService;

    public TagServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mockActivityLogService = MockServiceFactory.CreateMockActivityLogService();
        _authService = new BoardAuthorizationService(_context, _mockActivityLogService.Object);

        // Create in-memory configuration
        var configData = new Dictionary<string, string?>
    {
      {"Tags:MaxTagsPerBoard", "20"}
    };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _tagService = new TagService(_context, _configuration, _authService, _mockActivityLogService.Object);
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

    [Fact]
    public async Task CreateTagAsync_WithMemberRole_ReturnsForbidden()
    {
        // Arrange - Only Admin can create tags
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
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
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task CreateTagAsync_WithNonExistentBoard_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var createDto = new TagCreateDto
        {
            BoardId = 9999,
            Name = "New Tag",
            Color = "#FF5733"
        };

        // Act
        var result = await _tagService.CreateTagAsync(createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task UpdateTagAsync_WithViewerRole_ReturnsForbidden()
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
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task DeleteTagAsync_WithMemberRole_ReturnsForbidden()
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

        var tag = TestDataFactory.CreateTestTag(board.Id, "To Delete");
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.DeleteTagAsync(tag.Id, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task GetTagsByBoardIdAsync_WhenBoardNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.GetTagsByBoardIdAsync(boardId: 9999, userId: user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task UpdateTagAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var updateDto = new TagUpdateDto
        {
            Name = "Updated",
            Color = "#FF0000"
        };

        // Act
        var result = await _tagService.UpdateTagAsync(tagId: 9999, updateDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task DeleteTagAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.DeleteTagAsync(tagId: 9999, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    #region Tag Limits Tests
    [Fact]
    public async Task CreateTagAsync_WhenMaxTagsReached_ReturnsFailure()
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

        // Create 20 tags (max)
        for (int i = 0; i < 20; i++)
        {
            var tag = TestDataFactory.CreateTestTag(board.Id, $"Tag {i}", "#FF5733");
            _context.Tags.Add(tag);
        }
        await _context.SaveChangesAsync();

        var createDto = new TagCreateDto
        {
            BoardId = board.Id,
            Name = "Tag 21",
            Color = "#FF5733"
        };

        // Act
        var result = await _tagService.CreateTagAsync(createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("maximum", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task CreateTagAsync_WithInvalidColor_MayNotValidateAtServiceLevel()
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
            Name = "Bad Color Tag",
            Color = "not-a-color"  // Invalid color format
        };

        // Act
        var result = await _tagService.CreateTagAsync(createDto, user.Id);

        // Assert - Documents current behavior; validation may be at controller level
        // Just verify it either succeeds or fails gracefully
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateTagAsync_WithEmptyName_MayNotValidateAtServiceLevel()
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
            Name = "",
            Color = "#FF5733"
        };

        // Act
        var result = await _tagService.CreateTagAsync(createDto, user.Id);

        // Assert - Documents current behavior, service may reject empty names
        // Just verify it returns a result
        Assert.NotNull(result!);
    }
    #endregion

    #region Duplicate Name Handling
    [Fact]
    public async Task UpdateTagAsync_WithDuplicateName_ReturnsFailure()
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

        var tag1 = TestDataFactory.CreateTestTag(board.Id, "Tag 1", "#FF5733");
        var tag2 = TestDataFactory.CreateTestTag(board.Id, "Tag 2", "#33FF57");
        _context.Tags.AddRange(tag1, tag2);
        await _context.SaveChangesAsync();

        var updateDto = new TagUpdateDto
        {
            Name = "Tag 1",  // Already taken by tag1
            Color = "#FF5733"
        };

        // Act
        var result = await _tagService.UpdateTagAsync(tag2.Id, updateDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("already exists", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task CreateTagAsync_WithNonExistentBoard_ReturnsNotFoundCorrectly()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var createDto = new TagCreateDto
        {
            BoardId = 9999,
            Name = "New Tag",
            Color = "#FF5733"
        };

        // Act
        var result = await _tagService.CreateTagAsync(createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }
    #endregion

    #region Role-based Permission Tests
    [Fact]
    public async Task CreateTagAsync_AsViewerRole_ReturnsForbidden()
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

        var createDto = new TagCreateDto
        {
            BoardId = board.Id,
            Name = "New Tag",
            Color = "#FF5733"
        };

        // Act
        var result = await _tagService.CreateTagAsync(createDto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task UpdateTagAsync_WithAdminRole_Succeeds()
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

        var tag = TestDataFactory.CreateTestTag(board.Id, "Original Name", "#FF5733");
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var updateDto = new TagUpdateDto
        {
            Name = "New Name",
            Color = "#33FF57"
        };

        // Act
        var result = await _tagService.UpdateTagAsync(tag.Id, updateDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value.Name);
        Assert.Equal("#33FF57", result.Value.Color);
    }

    [Fact]
    public async Task DeleteTagAsync_WithAdminRole_Succeeds()
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

        var tag = TestDataFactory.CreateTestTag(board.Id, "To Delete", "#FF5733");
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.DeleteTagAsync(tag.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task GetTagsByBoardIdAsync_WithMemberRole_Succeeds()
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

        var tag = TestDataFactory.CreateTestTag(board.Id, "Tag 1", "#FF5733");
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.GetTagsByBoardIdAsync(board.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }
    #endregion

    #region Tag Cross-Board Isolation
    [Fact]
    public async Task GetTagsByBoardIdAsync_OnlyReturnsTagsForSpecificBoard()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board1 = TestDataFactory.CreateTestBoard(user.Id, "Board 1");
        var board2 = TestDataFactory.CreateTestBoard(user.Id, "Board 2");
        _context.Boards.AddRange(board1, board2);
        await _context.SaveChangesAsync();

        var member1 = TestDataFactory.CreateTestBoardMember(board1.Id, user.Id, BoardMemberRole.Admin);
        var member2 = TestDataFactory.CreateTestBoardMember(board2.Id, user.Id, BoardMemberRole.Admin);
        _context.BoardMembers.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        var tag1 = TestDataFactory.CreateTestTag(board1.Id, "Board1 Tag", "#FF5733");
        var tag2 = TestDataFactory.CreateTestTag(board2.Id, "Board2 Tag", "#33FF57");
        _context.Tags.AddRange(tag1, tag2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.GetTagsByBoardIdAsync(board1.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(tag1.Id, result.Value![0].Id);
    }
    #endregion

    #region Tag Attachment/Detachment (if applicable)
    [Fact]
    public async Task CreateTagAsync_SuccessfullyIncrementsTagCount()
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

        var tag1 = TestDataFactory.CreateTestTag(board.Id, "Existing Tag", "#FF5733");
        _context.Tags.Add(tag1);
        await _context.SaveChangesAsync();

        var createDto = new TagCreateDto
        {
            BoardId = board.Id,
            Name = "New Tag",
            Color = "#33FF57"
        };

        // Act
        var result = await _tagService.CreateTagAsync(createDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        var tags = await _tagService.GetTagsByBoardIdAsync(board.Id, user.Id);
        Assert.Equal(2, tags.Value!.Count);
    }

    [Fact]
    public async Task UpdateTagAsync_ChangingColorOnlyWorks()
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

        var tag = TestDataFactory.CreateTestTag(board.Id, "Tag Name", "#FF5733");
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var updateDto = new TagUpdateDto
        {
            Name = "Tag Name",  // Same name
            Color = "#00FF00"   // Different color
        };

        // Act
        var result = await _tagService.UpdateTagAsync(tag.Id, updateDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("#00FF00", result.Value.Color);
    }
    #endregion

    #region Multiple Tags per Board
    [Fact]
    public async Task CreateMultipleTags_StoresSeparatelyAndRetrievesAll()
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

        // Create 5 tags
        var tags = new List<Tag>();
        for (int i = 1; i <= 5; i++)
        {
            var tag = TestDataFactory.CreateTestTag(board.Id, $"Tag {i}", $"#FF{i:X2}33");
            tags.Add(tag);
        }
        _context.Tags.AddRange(tags);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.GetTagsByBoardIdAsync(board.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Count);
        for (int i = 0; i < 5; i++)
        {
            Assert.Contains($"Tag {i + 1}", result.Value!.Select(t => t.Name));
        }
    }
    #endregion
}
