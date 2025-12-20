using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StressTracker5001Server.Common;
using StressTracker5001Server.Controllers;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.Models;
using StressTracker5001Server.Services;
using StressTracker5001Server.Tests.Helpers;
using Xunit;

namespace StressTracker5001Server.Tests.Unit.Controllers;

public class TagsControllerTests
{
    private readonly Mock<ITagService> _mockTagService;
    private readonly Mock<IActivityLogService> _mockActivityLogService;
    private readonly TagsController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public TagsControllerTests()
    {
        _mockTagService = new Mock<ITagService>();
        _mockActivityLogService = MockServiceFactory.CreateMockActivityLogService();
        _controller = new TagsController();

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
    public async Task CreateTag_WithValidData_ReturnsCreatedTag()
    {
        // Arrange
        var createDto = new TagCreateDto
        {
            Name = "Bug",
            Color = "#FF0000",
            BoardId = 1
        };
        var tag = TestDataFactory.CreateTestTag(1, "Bug", "#FF0000");
        tag.Id = 1;

        _mockTagService
            .Setup(s => s.CreateTagAsync(createDto, 1))
            .ReturnsAsync(Result<Tag>.Success(tag));

        // Act
        var result = await _controller.CreateTag(createDto, _mockTagService.Object);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task CreateTag_ForNonOwnedBoard_ReturnsForbidden()
    {
        // Arrange
        var createDto = new TagCreateDto
        {
            Name = "Bug",
            Color = "#FF0000",
            BoardId = 1
        };

        _mockTagService
            .Setup(s => s.CreateTagAsync(createDto, 1))
            .ReturnsAsync(Result<Tag>.Forbidden("You don't have permission to create tags for this board"));

        // Act
        var result = await _controller.CreateTag(createDto, _mockTagService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreateTag_ForNonExistentBoard_ReturnsNotFound()
    {
        // Arrange
        var createDto = new TagCreateDto
        {
            Name = "Bug",
            Color = "#FF0000",
            BoardId = 999
        };

        _mockTagService
            .Setup(s => s.CreateTagAsync(createDto, 1))
            .ReturnsAsync(Result<Tag>.NotFound("Board not found"));

        // Act
        var result = await _controller.CreateTag(createDto, _mockTagService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateTag_WithValidData_ReturnsUpdatedTag()
    {
        // Arrange
        var updateDto = new TagUpdateDto
        {
            Name = "Updated Bug",
            Color = "#00FF00"
        };
        var tag = TestDataFactory.CreateTestTag(1, "Updated Bug", "#00FF00");
        tag.Id = 1;

        _mockTagService
            .Setup(s => s.UpdateTagAsync(1, updateDto, 1))
            .ReturnsAsync(Result<Tag>.Success(tag));

        // Act
        var result = await _controller.UpdateTag(1, updateDto, _mockTagService.Object);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateTag_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var updateDto = new TagUpdateDto
        {
            Name = "Updated Bug",
            Color = "#00FF00"
        };

        _mockTagService
            .Setup(s => s.UpdateTagAsync(1, updateDto, 1))
            .ReturnsAsync(Result<Tag>.Forbidden("You don't have permission to update this tag"));

        // Act
        var result = await _controller.UpdateTag(1, updateDto, _mockTagService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateTag_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new TagUpdateDto
        {
            Name = "Updated Bug",
            Color = "#00FF00"
        };

        _mockTagService
            .Setup(s => s.UpdateTagAsync(999, updateDto, 1))
            .ReturnsAsync(Result<Tag>.NotFound("Tag not found"));

        // Act
        var result = await _controller.UpdateTag(999, updateDto, _mockTagService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteTag_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockTagService
            .Setup(s => s.DeleteTagAsync(1, 1))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.DeleteTag(1, _mockTagService.Object);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task DeleteTag_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        _mockTagService
            .Setup(s => s.DeleteTagAsync(1, 1))
            .ReturnsAsync(Result<bool>.Forbidden("You don't have permission to delete this tag"));

        // Act
        var result = await _controller.DeleteTag(1, _mockTagService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteTag_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockTagService
            .Setup(s => s.DeleteTagAsync(999, 1))
            .ReturnsAsync(Result<bool>.NotFound("Tag not found"));

        // Act
        var result = await _controller.DeleteTag(999, _mockTagService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteTag_ThatIsInUse_ReturnsError()
    {
        // Arrange
        _mockTagService
            .Setup(s => s.DeleteTagAsync(1, 1))
            .ReturnsAsync(Result<bool>.Failure("Cannot delete tag that is assigned to cards", 400));

        // Act
        var result = await _controller.DeleteTag(1, _mockTagService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }
}
