using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StressTracker5001Server.Common;
using StressTracker5001Server.Controllers;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Models;
using StressTracker5001Server.Services;
using StressTracker5001Server.Tests.Helpers;
using Xunit;

namespace StressTracker5001Server.Tests.Unit.Controllers;

public class CardsControllerTests
{
    private readonly Mock<ICardService> _mockCardService;
    private readonly Mock<ICommentService> _mockCommentService;
    private readonly CardsController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public CardsControllerTests()
    {
        _mockCardService = new Mock<ICardService>();
        _mockCommentService = new Mock<ICommentService>();
        _controller = new CardsController();

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
    public async Task GetCard_WithValidId_ReturnsCardDetails()
    {
        // Arrange
        var card = TestDataFactory.CreateTestCard(1, "Test Card");
        card.Id = 1;
        card.CreatedById = 1;

        _mockCardService
            .Setup(s => s.GetCardDetailsByIdAsync(1, 1))
            .ReturnsAsync(Result<Card>.Success(card));

        // Act
        var result = await _controller.GetCard(1, _mockCardService.Object);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetCard_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockCardService
            .Setup(s => s.GetCardDetailsByIdAsync(999, 1))
            .ReturnsAsync(Result<Card>.NotFound("Card not found"));

        // Act
        var result = await _controller.GetCard(999, _mockCardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateCard_WithValidData_ReturnsUpdatedCard()
    {
        // Arrange
        var updateDto = new UpdateCardDto
        {
            Title = "Updated Title",
            Description = "Updated Description"
        };
        var updatedCard = TestDataFactory.CreateTestCard(1, "Updated Title");
        updatedCard.Id = 1;

        _mockCardService
            .Setup(s => s.UpdateCardAsync(1, updateDto, 1))
            .ReturnsAsync(Result<Card>.Success(updatedCard));

        // Act
        var result = await _controller.UpdateCard(1, updateDto, _mockCardService.Object);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateCard_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var updateDto = new UpdateCardDto
        {
            Title = "Updated Title"
        };

        _mockCardService
            .Setup(s => s.UpdateCardAsync(1, updateDto, 1))
            .ReturnsAsync(Result<Card>.Forbidden("You don't have permission to update this card"));

        // Act
        var result = await _controller.UpdateCard(1, updateDto, _mockCardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task MoveCard_WithValidColumn_ReturnsMovedCard()
    {
        // Arrange
        var moveDto = new MoveCardDto
        {
            NewColumnId = 2,
            NewPosition = 0
        };
        var movedCard = TestDataFactory.CreateTestCard(2, "Test Card");
        movedCard.Id = 1;
        movedCard.Position = 0;

        _mockCardService
            .Setup(s => s.MoveCardAsync(1, moveDto, 1))
            .ReturnsAsync(Result<Card>.Success(movedCard));

        // Act
        var result = await _controller.MoveCard(1, moveDto, _mockCardService.Object);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task MoveCard_ToInvalidColumn_ReturnsNotFound()
    {
        // Arrange
        var moveDto = new MoveCardDto
        {
            NewColumnId = 999,
            NewPosition = 0
        };

        _mockCardService
            .Setup(s => s.MoveCardAsync(1, moveDto, 1))
            .ReturnsAsync(Result<Card>.NotFound("Column not found"));

        // Act
        var result = await _controller.MoveCard(1, moveDto, _mockCardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task AssignTagsToCard_WithValidTags_ReturnsCardWithTags()
    {
        // Arrange
        var tagsDto = new CardAssignTagsDto
        {
            Tags = new List<int> { 1, 2, 3 }
        };
        var card = TestDataFactory.CreateTestCard(1, "Test Card");
        card.Id = 1;

        _mockCardService
            .Setup(s => s.AssignTagsToCardAsync(1, tagsDto.Tags, 1))
            .ReturnsAsync(Result<Card>.Success(card));

        // Act
        var result = await _controller.AssignTagsToCard(1, tagsDto, _mockCardService.Object);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetCardComments_WithValidPagination_ReturnsPagedComments()
    {
        // Arrange
        var comments = new List<Comment>
        {
            TestDataFactory.CreateTestComment(1, 1, "Comment 1"),
            TestDataFactory.CreateTestComment(1, 1, "Comment 2")
        };
        comments[0].Id = 1;
        comments[1].Id = 2;

        _mockCardService
            .Setup(s => s.GetCommentsByCardIdAsync(1, 1, 1, 10))
            .ReturnsAsync(Result<List<Comment>>.Success(comments));

        _mockCardService
            .Setup(s => s.HasMoreCommentsAsync(1, 1, 1, 10))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _controller.GetCardComments(1, _mockCardService.Object, 1, 10);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetCardComments_WithInvalidPage_NormalizesToOne()
    {
        // Arrange
        var comments = new List<Comment>();

        _mockCardService
            .Setup(s => s.GetCommentsByCardIdAsync(1, 1, 1, 10))
            .ReturnsAsync(Result<List<Comment>>.Success(comments));

        _mockCardService
            .Setup(s => s.HasMoreCommentsAsync(1, 1, 1, 10))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act - invalid page should be normalized to 1
        var result = await _controller.GetCardComments(1, _mockCardService.Object, 0, 10);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockCardService.Verify(s => s.GetCommentsByCardIdAsync(1, 1, 1, 10), Times.Once);
    }

    [Fact]
    public async Task GetCardComments_WithInvalidPageSize_NormalizesToTen()
    {
        // Arrange
        var comments = new List<Comment>();

        _mockCardService
            .Setup(s => s.GetCommentsByCardIdAsync(1, 1, 1, 10))
            .ReturnsAsync(Result<List<Comment>>.Success(comments));

        _mockCardService
            .Setup(s => s.HasMoreCommentsAsync(1, 1, 1, 10))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act - invalid pageSize should be normalized to 10
        var result = await _controller.GetCardComments(1, _mockCardService.Object, 1, 0);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockCardService.Verify(s => s.GetCommentsByCardIdAsync(1, 1, 1, 10), Times.Once);
    }

    [Fact]
    public async Task AddCommentToCard_WithValidData_ReturnsCreatedComment()
    {
        // Arrange
        var createDto = new CreateCommentDto
        {
            Content = "New comment"
        };
        var comment = TestDataFactory.CreateTestComment(1, 1, "New comment");
        comment.Id = 1;

        _mockCardService
            .Setup(s => s.AddCommentToCardAsync(1, createDto, 1))
            .ReturnsAsync(Result<Comment>.Success(comment));

        // Act
        var result = await _controller.AddCommentToCard(1, createDto, _mockCardService.Object, _mockCommentService.Object);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task AddCommentToCard_ToNonExistentCard_ReturnsNotFound()
    {
        // Arrange
        var createDto = new CreateCommentDto
        {
            Content = "New comment"
        };

        _mockCardService
            .Setup(s => s.AddCommentToCardAsync(999, createDto, 1))
            .ReturnsAsync(Result<Comment>.NotFound("Card not found"));

        // Act
        var result = await _controller.AddCommentToCard(999, createDto, _mockCardService.Object, _mockCommentService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteCard_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockCardService
            .Setup(s => s.DeleteCardAsync(1, 1))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.DeleteCard(1, _mockCardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(204, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteCard_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        _mockCardService
            .Setup(s => s.DeleteCardAsync(1, 1))
            .ReturnsAsync(Result<bool>.Forbidden("You don't have permission to delete this card"));

        // Act
        var result = await _controller.DeleteCard(1, _mockCardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteCard_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockCardService
            .Setup(s => s.DeleteCardAsync(999, 1))
            .ReturnsAsync(Result<bool>.NotFound("Card not found"));

        // Act
        var result = await _controller.DeleteCard(999, _mockCardService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }
}
