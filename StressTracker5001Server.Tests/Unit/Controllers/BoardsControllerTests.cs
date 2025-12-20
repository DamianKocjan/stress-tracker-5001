using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using StressTracker5001Server.Controllers;
using StressTracker5001Server.Services;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.Common;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Controllers;

public class BoardsControllerTests
{
    private readonly Mock<IBoardService> _mockBoardService;
    private readonly Mock<IActivityLogService> _mockActivityLogService;
    private readonly BoardsController _controller;
    private const int TestUserId = 1;

    public BoardsControllerTests()
    {
        _mockBoardService = new Mock<IBoardService>();
        _mockActivityLogService = MockServiceFactory.CreateMockActivityLogService();
        _controller = new BoardsController();

        // Setup user claims for authorization
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task CreateBoard_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var createDto = new CreateBoardDto
        {
            Name = "Test Board",
            Description = "Test Description"
        };

        var board = TestDataFactory.CreateTestBoard(TestUserId, createDto.Name, createDto.Description);
        board.Id = 1;

        _mockBoardService
            .Setup(s => s.CreateBoardAsync(It.IsAny<CreateBoardDto>(), TestUserId))
            .Returns(Task.FromResult(Result<Board>.Success(board)));

        _mockBoardService
            .Setup(s => s.GetBoardByIdAsync(board.Id, TestUserId))
            .ReturnsAsync(Result<Board>.Success(board));

        // Act
        var result = await _controller.CreateBoard(createDto, _mockBoardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public async Task CreateBoard_WhenServiceFails_ReturnsFailureResult()
    {
        // Arrange
        var createDto = new CreateBoardDto
        {
            Name = "Test Board"
        };

        _mockBoardService
            .Setup(s => s.CreateBoardAsync(It.IsAny<CreateBoardDto>(), TestUserId))
            .Returns(Task.FromResult(Result<Board>.Failure("Failed to create board", 400)));

        // Act
        var result = await _controller.CreateBoard(createDto, _mockBoardService.Object);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetBoards_ReturnsOwnedBoards()
    {
        // Arrange
        var boards = new List<Board>
        {
            TestDataFactory.CreateTestBoard(TestUserId, "Board 1"),
            TestDataFactory.CreateTestBoard(TestUserId, "Board 2")
        };

        _mockBoardService
            .Setup(s => s.GetOwnedBoardsAsync(TestUserId))
            .Returns(Task.FromResult(Result<List<Board>>.Success(boards)));

        // Act
        var result = await _controller.GetBoards(_mockBoardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public async Task GetBoard_WithValidId_ReturnsBoard()
    {
        // Arrange
        var boardId = 1;
        var boardDetailsDto = new BoardDetailsDto
        {
            Id = boardId,
            Name = "Test Board",
            Description = "Test Description",
            Owner = new UserDto
            {
                Id = TestUserId,
                Username = "testuser",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            Tags = new List<TagDto>(),
            Columns = new List<ColumnDto>(),
            Cards = new List<CardDto>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockBoardService
            .Setup(s => s.GetBoardWithColumnsAndCardsAsync(boardId, TestUserId))
            .Returns(Task.FromResult(Result<BoardDetailsDto>.Success(boardDetailsDto)));

        // Act
        var result = await _controller.GetBoard(boardId, _mockBoardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public async Task GetBoard_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var boardId = 999;

        _mockBoardService
            .Setup(s => s.GetBoardWithColumnsAndCardsAsync(boardId, TestUserId))
            .Returns(Task.FromResult(Result<BoardDetailsDto>.NotFound($"Board with ID {boardId} not found")));

        // Act
        var result = await _controller.GetBoard(boardId, _mockBoardService.Object);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task UpdateBoard_WithValidData_ReturnsUpdatedBoard()
    {
        // Arrange
        var boardId = 1;
        var updateDto = new UpdateBoardDto
        {
            Name = "Updated Board",
            Description = "Updated Description"
        };

        var updatedBoard = TestDataFactory.CreateTestBoard(TestUserId, updateDto.Name, updateDto.Description);
        updatedBoard.Id = boardId;

        _mockBoardService
            .Setup(s => s.UpdateBoardAsync(boardId, It.IsAny<UpdateBoardDto>(), TestUserId))
            .Returns(Task.FromResult(Result<Board>.Success(updatedBoard)));

        // Act
        var result = await _controller.UpdateBoard(boardId, updateDto, _mockBoardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public async Task DeleteBoard_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var boardId = 1;

        _mockBoardService
            .Setup(s => s.DeleteBoardAsync(boardId, TestUserId))
            .Returns(Task.FromResult(Result<bool>.Success(true)));

        // Act
        var result = await _controller.DeleteBoard(boardId, _mockBoardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(204, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public async Task DeleteBoard_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var boardId = 999;

        _mockBoardService
            .Setup(s => s.DeleteBoardAsync(boardId, TestUserId))
            .Returns(Task.FromResult(Result<bool>.NotFound($"Board with ID {boardId} not found")));

        // Act
        var result = await _controller.DeleteBoard(boardId, _mockBoardService.Object);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task CreateBoard_VerifiesUserAuthentication()
    {
        // Arrange
        var createDto = new CreateBoardDto { Name = "Test" };

        // Remove user claims to simulate unauthenticated request
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

        // Act
        var result = await _controller.CreateBoard(createDto, _mockBoardService.Object);

        // Assert
        var unauthorizedResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }
}
