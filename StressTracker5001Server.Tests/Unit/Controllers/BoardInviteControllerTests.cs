using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using StressTracker5001Server.Controllers;
using StressTracker5001Server.Services;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.BoardInvite;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Controllers;

public class BoardInviteControllerTests
{
    private readonly Mock<IBoardInviteService> _mockBoardInviteService;
    private readonly Mock<IActivityLogService> _mockActivityLogService;
    private readonly BoardInviteController _controller;
    private readonly UserDto _testUserDto;

    public BoardInviteControllerTests()
    {
        _mockBoardInviteService = new Mock<IBoardInviteService>();
        _mockActivityLogService = MockServiceFactory.CreateMockActivityLogService();
        _controller = new BoardInviteController();

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

        // Setup test user DTO
        _testUserDto = new UserDto
        {
            Id = 1,
            Username = "testuser",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task JoinBoard_WithValidToken_ReturnsOkResult()
    {
        // Arrange
        var userId = 1;
        var inviteToken = "valid-invite-token-12345";
        var board = TestDataFactory.CreateTestBoard(1, "Test Board");
        var boardInviteDto = new BoardInviteDto
        {
            Id = 1,
            Token = inviteToken,
            Role = (int)BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            GeneratedByUser = _testUserDto
        };

        _mockBoardInviteService.Setup(s => s.AcceptInviteAsync(userId, inviteToken))
            .ReturnsAsync(StressTracker5001Server.Common.Result<Board>.Success(board));

        // Act
        var result = await _controller.JoinBoard(boardInviteDto, _mockBoardInviteService.Object);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(200, objectResult.StatusCode);
    }

    [Fact]
    public async Task JoinBoard_WithExpiredToken_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var expiredToken = "expired-token-12345";
        var boardInviteDto = new BoardInviteDto
        {
            Id = 1,
            Token = expiredToken,
            Role = (int)BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            GeneratedByUser = _testUserDto
        };

        _mockBoardInviteService.Setup(s => s.AcceptInviteAsync(userId, expiredToken))
            .ReturnsAsync(StressTracker5001Server.Common.Result<Board>.Failure("Invite token has expired", 400));

        // Act
        var result = await _controller.JoinBoard(boardInviteDto, _mockBoardInviteService.Object);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task JoinBoard_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var invalidToken = "invalid-token";
        var boardInviteDto = new BoardInviteDto
        {
            Id = 1,
            Token = invalidToken,
            Role = (int)BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            GeneratedByUser = _testUserDto
        };

        _mockBoardInviteService.Setup(s => s.AcceptInviteAsync(userId, invalidToken))
            .ReturnsAsync(StressTracker5001Server.Common.Result<Board>.Failure("Invalid invite token", 400));

        // Act
        var result = await _controller.JoinBoard(boardInviteDto, _mockBoardInviteService.Object);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task JoinBoard_WhenUserAlreadyMember_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var inviteToken = "valid-token";
        var boardInviteDto = new BoardInviteDto
        {
            Id = 1,
            Token = inviteToken,
            Role = (int)BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            GeneratedByUser = _testUserDto
        };

        _mockBoardInviteService.Setup(s => s.AcceptInviteAsync(userId, inviteToken))
            .ReturnsAsync(StressTracker5001Server.Common.Result<Board>.Failure("User is already a member of this board", 400));

        // Act
        var result = await _controller.JoinBoard(boardInviteDto, _mockBoardInviteService.Object);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task RevokeInvite_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var inviteId = 1;
        var userId = 1;

        _mockBoardInviteService.Setup(s => s.RevokeInviteAsync(inviteId, userId))
            .ReturnsAsync(StressTracker5001Server.Common.Result<bool>.Success(true));

        // Act
        var result = await _controller.RevokeInvite(inviteId, _mockBoardInviteService.Object);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(204, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task RevokeInvite_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var inviteId = 1;
        var userId = 1;

        _mockBoardInviteService.Setup(s => s.RevokeInviteAsync(inviteId, userId))
            .ReturnsAsync(StressTracker5001Server.Common.Result<bool>.Forbidden("You do not have permission to revoke this invite"));

        // Act
        var result = await _controller.RevokeInvite(inviteId, _mockBoardInviteService.Object);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task RevokeInvite_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var inviteId = 9999;
        var userId = 1;

        _mockBoardInviteService.Setup(s => s.RevokeInviteAsync(inviteId, userId))
            .ReturnsAsync(StressTracker5001Server.Common.Result<bool>.NotFound("Invite not found"));

        // Act
        var result = await _controller.RevokeInvite(inviteId, _mockBoardInviteService.Object);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task RevokeInvite_WhenAlreadyRevoked_ReturnsBadRequest()
    {
        // Arrange
        var inviteId = 1;
        var userId = 1;

        _mockBoardInviteService.Setup(s => s.RevokeInviteAsync(inviteId, userId))
            .ReturnsAsync(StressTracker5001Server.Common.Result<bool>.Failure("Invite has already been revoked", 400));

        // Act
        var result = await _controller.RevokeInvite(inviteId, _mockBoardInviteService.Object);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, ((ObjectResult)result).StatusCode);
    }
}
