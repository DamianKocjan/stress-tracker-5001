using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using StressTracker5001Server.Controllers;
using StressTracker5001Server.Services;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Controllers;

public class ColumnsControllerTests
{
  private readonly Mock<IColumnService> _mockColumnService;
  private readonly Mock<ICardService> _mockCardService;
  private readonly ColumnsController _controller;

  public ColumnsControllerTests()
  {
    _mockColumnService = new Mock<IColumnService>();
    _mockCardService = new Mock<ICardService>();
    _controller = new ColumnsController();

    // Setup User claim for authenticated requests
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
    var identity = new ClaimsIdentity(claims, "TestScheme");
    var principal = new ClaimsPrincipal(identity);
    _controller.ControllerContext = new ControllerContext
    {
      HttpContext = new DefaultHttpContext { User = principal }
    };
  }

  [Fact]
  public async Task UpdateColumn_WithValidData_ReturnsOkResult()
  {
    // Arrange
    var columnId = 1;
    var userId = 1;
    var column = TestDataFactory.CreateTestColumn(1, "Updated Column");
    var updateDto = new UpdateColumnDto
    {
      Name = "Updated Column",
      WipLimit = 10
    };

    _mockColumnService.Setup(s => s.UpdateColumnAsync(columnId, updateDto, userId))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Column>.Success(column));

    // Act
    var result = await _controller.UpdateColumn(columnId, updateDto, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    var objectResult = result as ObjectResult;
    Assert.NotNull(objectResult);
    Assert.Equal(200, objectResult.StatusCode);
  }

  [Fact]
  public async Task UpdateColumn_WithoutPermission_ReturnsForbidden()
  {
    // Arrange
    var columnId = 1;
    var userId = 1;
    var updateDto = new UpdateColumnDto
    {
      Name = "Updated Column"
    };

    _mockColumnService.Setup(s => s.UpdateColumnAsync(columnId, updateDto, userId))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Column>.Forbidden("Access denied"));

    // Act
    var result = await _controller.UpdateColumn(columnId, updateDto, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    Assert.Equal(403, ((ObjectResult)result).StatusCode);
  }

  [Fact]
  public async Task UpdateColumn_WhenNotFound_ReturnsNotFound()
  {
    // Arrange
    var columnId = 9999;
    var userId = 1;
    var updateDto = new UpdateColumnDto
    {
      Name = "Updated Column"
    };

    _mockColumnService.Setup(s => s.UpdateColumnAsync(columnId, updateDto, userId))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Column>.NotFound("Column not found"));

    // Act
    var result = await _controller.UpdateColumn(columnId, updateDto, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    Assert.Equal(404, ((ObjectResult)result).StatusCode);
  }

  [Fact]
  public async Task MoveColumn_WithValidPosition_ReturnsOkResult()
  {
    // Arrange
    var columnId = 1;
    var userId = 1;
    var column = TestDataFactory.CreateTestColumn(1, "Test Column", position: 2);
    var moveDto = new MoveColumnDto { NewPosition = 2 };

    _mockColumnService.Setup(s => s.MoveColumnAsync(columnId, moveDto.NewPosition, userId))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Column>.Success(column));

    // Act
    var result = await _controller.MoveColumn(columnId, moveDto, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    var objectResult = result as ObjectResult;
    Assert.NotNull(objectResult);
    Assert.Equal(200, objectResult.StatusCode);
  }

  [Fact]
  public async Task MoveColumn_WithInvalidPosition_ReturnsBadRequest()
  {
    // Arrange
    var columnId = 1;
    var userId = 1;
    var moveDto = new MoveColumnDto { NewPosition = 999 };

    _mockColumnService.Setup(s => s.MoveColumnAsync(columnId, moveDto.NewPosition, userId))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Column>.Failure("Position out of range", 400));

    // Act
    var result = await _controller.MoveColumn(columnId, moveDto, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    Assert.Equal(400, ((ObjectResult)result).StatusCode);
  }

  [Fact]
  public async Task DeleteColumn_WithValidId_ReturnsNoContent()
  {
    // Arrange
    var columnId = 1;
    var userId = 1;

    _mockColumnService.Setup(s => s.DeleteColumnAsync(columnId, userId))
        .ReturnsAsync(StressTracker5001Server.Common.Result<bool>.Success(true));

    // Act
    var result = await _controller.DeleteColumn(columnId, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    Assert.Equal(204, ((ObjectResult)result).StatusCode);
  }

  [Fact]
  public async Task DeleteColumn_WithoutPermission_ReturnsForbidden()
  {
    // Arrange
    var columnId = 1;
    var userId = 1;

    _mockColumnService.Setup(s => s.DeleteColumnAsync(columnId, userId))
        .ReturnsAsync(StressTracker5001Server.Common.Result<bool>.Forbidden("Access denied"));

    // Act
    var result = await _controller.DeleteColumn(columnId, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    Assert.Equal(403, ((ObjectResult)result).StatusCode);
  }

  [Fact]
  public async Task CreateCardInColumn_WithValidData_ReturnsCreatedCard()
  {
    // Arrange
    var columnId = 1;
    var userId = 1;
    var column = TestDataFactory.CreateTestColumn(columnId, "To Do");
    var card = TestDataFactory.CreateTestCard(columnId, "New Card", createdById: userId);
    var createDto = new CreateCardDto
    {
      Title = "New Card",
      Description = "Card description"
    };

    _mockColumnService.Setup(s => s.GetColumnByIdAsync(columnId, userId, BoardMemberRole.Viewer))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Column>.Success(column));
    _mockCardService.Setup(s => s.CreateCardAsync(columnId, createDto, userId))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Card>.Success(card));

    // Act
    var result = await _controller.CreateCardInColumn(columnId, createDto, _mockCardService.Object, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    var objectResult = result as ObjectResult;
    Assert.NotNull(objectResult);
    Assert.Equal(200, objectResult.StatusCode);
  }

  [Fact]
  public async Task CreateCardInColumn_WithoutPermission_ReturnsForbidden()
  {
    // Arrange
    var columnId = 1;
    var userId = 1;
    var createDto = new CreateCardDto { Title = "New Card" };

    _mockColumnService.Setup(s => s.GetColumnByIdAsync(columnId, userId, BoardMemberRole.Viewer))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Column>.Forbidden("Access denied"));

    // Act
    var result = await _controller.CreateCardInColumn(columnId, createDto, _mockCardService.Object, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    Assert.Equal(403, ((ObjectResult)result).StatusCode);
  }

  [Fact]
  public async Task CreateCardInColumn_WithWipLimitExceeded_ReturnsBadRequest()
  {
    // Arrange
    var columnId = 1;
    var userId = 1;
    var column = TestDataFactory.CreateTestColumn(columnId, "Full Column", wipLimit: 1);
    var createDto = new CreateCardDto { Title = "New Card" };

    _mockColumnService.Setup(s => s.GetColumnByIdAsync(columnId, userId, BoardMemberRole.Viewer))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Column>.Success(column));
    _mockCardService.Setup(s => s.CreateCardAsync(columnId, createDto, userId))
        .ReturnsAsync(StressTracker5001Server.Common.Result<Card>.Failure("WIP limit exceeded", 400));

    // Act
    var result = await _controller.CreateCardInColumn(columnId, createDto, _mockCardService.Object, _mockColumnService.Object);

    // Assert
    Assert.IsType<ObjectResult>(result);
    Assert.Equal(400, ((ObjectResult)result).StatusCode);
  }
}
