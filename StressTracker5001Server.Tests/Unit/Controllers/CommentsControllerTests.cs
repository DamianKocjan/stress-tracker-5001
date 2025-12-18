using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StressTracker5001Server.Common;
using StressTracker5001Server.Controllers;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.Models;
using StressTracker5001Server.Services;
using StressTracker5001Server.Tests.Helpers;
using Xunit;

namespace StressTracker5001Server.Tests.Unit.Controllers;

public class CommentsControllerTests
{
    private readonly Mock<ICommentService> _mockCommentService;
    private readonly CommentsController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public CommentsControllerTests()
    {
        _mockCommentService = new Mock<ICommentService>();
        _controller = new CommentsController();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1")
        };
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _userPrincipal }
        };
    }

    [Fact]
    public async Task UpdateComment_WithValidData_ReturnsUpdatedComment()
    {
        // Arrange
        var updateDto = new UpdateCommentDto
        {
            Content = "Updated comment content"
        };
        var comment = TestDataFactory.CreateTestComment(1, 1, "Updated comment content");
        comment.Id = 1;

        _mockCommentService
            .Setup(s => s.UpdateCommentAsync(1, updateDto, 1))
            .ReturnsAsync(Result<Comment>.Success(comment));

        // Act
        var result = await _controller.UpdateComment(1, updateDto, _mockCommentService.Object);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateComment_ByNonAuthor_ReturnsForbidden()
    {
        // Arrange
        var updateDto = new UpdateCommentDto
        {
            Content = "Updated comment content"
        };

        _mockCommentService
            .Setup(s => s.UpdateCommentAsync(1, updateDto, 1))
            .ReturnsAsync(Result<Comment>.Forbidden("You can only update your own comments"));

        // Act
        var result = await _controller.UpdateComment(1, updateDto, _mockCommentService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateComment_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateCommentDto
        {
            Content = "Updated comment content"
        };

        _mockCommentService
            .Setup(s => s.UpdateCommentAsync(999, updateDto, 1))
            .ReturnsAsync(Result<Comment>.NotFound("Comment not found"));

        // Act
        var result = await _controller.UpdateComment(999, updateDto, _mockCommentService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateComment_WithEmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateCommentDto
        {
            Content = ""
        };

        _mockCommentService
            .Setup(s => s.UpdateCommentAsync(1, updateDto, 1))
            .ReturnsAsync(Result<Comment>.Failure("Comment content cannot be empty", 400));

        // Act
        var result = await _controller.UpdateComment(1, updateDto, _mockCommentService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockCommentService
            .Setup(s => s.DeleteCommentAsync(1, 1))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.DeleteComment(1, _mockCommentService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(204, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_ByNonAuthor_ReturnsForbidden()
    {
        // Arrange
        _mockCommentService
            .Setup(s => s.DeleteCommentAsync(1, 1))
            .ReturnsAsync(Result<bool>.Forbidden("You can only delete your own comments"));

        // Act
        var result = await _controller.DeleteComment(1, _mockCommentService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockCommentService
            .Setup(s => s.DeleteCommentAsync(999, 1))
            .ReturnsAsync(Result<bool>.NotFound("Comment not found"));

        // Act
        var result = await _controller.DeleteComment(999, _mockCommentService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_WithBoardMemberPermission_ReturnsNoContent()
    {
        // Arrange - Board admin/member can delete any comment on their board
        _mockCommentService
            .Setup(s => s.DeleteCommentAsync(1, 1))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.DeleteComment(1, _mockCommentService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(204, objectResult.StatusCode);
    }
}
