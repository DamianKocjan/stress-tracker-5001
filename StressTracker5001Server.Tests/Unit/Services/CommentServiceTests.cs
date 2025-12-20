using Xunit;
using Moq;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class CommentServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BoardAuthorizationService _authService;
    private readonly CommentService _commentService;
    private readonly Mock<IActivityLogService> _mockActivityLogService;

    public CommentServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mockActivityLogService = MockServiceFactory.CreateMockActivityLogService();
        _authService = new BoardAuthorizationService(_context, _mockActivityLogService.Object);
        _commentService = new CommentService(_context, _authService, _mockActivityLogService.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetCommentByIdAsync_WithValidComment_ReturnsComment()
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

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card");
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, user.Id, "Test Comment");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _commentService.GetCommentByIdAsync(comment.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(comment.Id, result.Value.Id);
        Assert.Equal("Test Comment", result.Value.Content);
    }

    [Fact]
    public async Task UpdateCommentAsync_WithValidData_UpdatesComment()
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

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card");
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, user.Id, "Old Comment");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCommentDto
        {
            Content = "Updated Comment"
        };

        // Act
        var result = await _commentService.UpdateCommentAsync(comment.Id, updateDto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated Comment", result.Value.Content);
    }

    [Fact]
    public async Task DeleteCommentAsync_WithValidComment_DeletesComment()
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

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card");
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, user.Id, "To Delete");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _commentService.DeleteCommentAsync(comment.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        // Verify comment was deleted
        var deletedComment = await _context.Comments.FindAsync(comment.Id);
        Assert.Null(deletedComment);
    }

    [Fact]
    public async Task GetCommentByIdAsync_WithoutPermission_ReturnsForbidden()
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

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card");
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, owner.Id, "Test Comment");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _commentService.GetCommentByIdAsync(comment.Id, nonMember.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task UpdateCommentAsync_ByNonAuthor_ReturnsForbidden()
    {
        // Arrange - Only the author or board admin can edit a comment
        var author = TestDataFactory.CreateTestUser(email: "author@example.com");
        var otherMember = TestDataFactory.CreateTestUser(email: "other@example.com");
        _context.Users.AddRange(author, otherMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(author.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMemberAuthor = TestDataFactory.CreateTestBoardMember(board.Id, author.Id, BoardMemberRole.Member);
        var boardMemberOther = TestDataFactory.CreateTestBoardMember(board.Id, otherMember.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(boardMemberAuthor, boardMemberOther);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: author.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, author.Id, "Original");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCommentDto { Content = "Updated by Other" };

        // Act
        var result = await _commentService.UpdateCommentAsync(comment.Id, updateDto, otherMember.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task DeleteCommentAsync_ByNonAuthor_ReturnsForbidden()
    {
        // Arrange - Only the author or board admin can delete a comment
        var author = TestDataFactory.CreateTestUser(email: "author@example.com");
        var otherMember = TestDataFactory.CreateTestUser(email: "other@example.com");
        _context.Users.AddRange(author, otherMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(author.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMemberAuthor = TestDataFactory.CreateTestBoardMember(board.Id, author.Id, BoardMemberRole.Member);
        var boardMemberOther = TestDataFactory.CreateTestBoardMember(board.Id, otherMember.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(boardMemberAuthor, boardMemberOther);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: author.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, author.Id, "To Delete");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _commentService.DeleteCommentAsync(comment.Id, otherMember.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task GetCommentByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _commentService.GetCommentByIdAsync(commentId: 9999, userId: user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    #region Additional Author Tests
    [Fact]
    public async Task DeleteCommentAsync_ByAuthor_Succeeds()
    {
        // Arrange
        var author = TestDataFactory.CreateTestUser(email: "author@example.com");
        _context.Users.Add(author);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(author.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var authorMember = TestDataFactory.CreateTestBoardMember(board.Id, author.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(authorMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: author.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, author.Id, "My Comment");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _commentService.DeleteCommentAsync(comment.Id, author.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task UpdateCommentAsync_ByAuthor_Succeeds()
    {
        // Arrange
        var author = TestDataFactory.CreateTestUser(email: "author@example.com");
        _context.Users.Add(author);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(author.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var authorMember = TestDataFactory.CreateTestBoardMember(board.Id, author.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(authorMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: author.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, author.Id, "Original");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCommentDto { Content = "Updated by Author" };

        // Act
        var result = await _commentService.UpdateCommentAsync(comment.Id, updateDto, author.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated by Author", result.Value.Content);
    }

    [Fact]
    public async Task UpdateCommentAsync_ByNonAuthor_ReturnsNull()
    {
        // Arrange
        var author = TestDataFactory.CreateTestUser(email: "author@example.com");
        var otherUser = TestDataFactory.CreateTestUser(email: "other@example.com");
        _context.Users.AddRange(author, otherUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(author.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var authorMember = TestDataFactory.CreateTestBoardMember(board.Id, author.Id, BoardMemberRole.Member);
        var otherMember = TestDataFactory.CreateTestBoardMember(board.Id, otherUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(authorMember, otherMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: author.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, author.Id, "Original");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCommentDto { Content = "Unauthorized Update" };

        // Act
        var result = await _commentService.UpdateCommentAsync(comment.Id, updateDto, otherUser.Id);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteCommentAsync_ByNonAuthor_ReturnsFalse()
    {
        // Arrange
        var author = TestDataFactory.CreateTestUser(email: "author@example.com");
        var otherUser = TestDataFactory.CreateTestUser(email: "other@example.com");
        _context.Users.AddRange(author, otherUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(author.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var authorMember = TestDataFactory.CreateTestBoardMember(board.Id, author.Id, BoardMemberRole.Member);
        var otherMember = TestDataFactory.CreateTestBoardMember(board.Id, otherUser.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(authorMember, otherMember);
        await _context.SaveChangesAsync();

        var column = TestDataFactory.CreateTestColumn(board.Id, "To Do");
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var card = TestDataFactory.CreateTestCard(column.Id, "Test Card", createdById: author.Id);
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        var comment = TestDataFactory.CreateTestComment(card.Id, author.Id, "To Delete");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        // Act
        var result = await _commentService.DeleteCommentAsync(comment.Id, otherUser.Id);

        // Assert
        Assert.False(result.IsSuccess);
    }
    #endregion
}
