using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using StressTracker5001Server.Controllers;
using StressTracker5001Server.Services;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.Auth;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Common;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IActivityLogService> _mockActivityLogService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockTokenService = new Mock<ITokenService>();
        _mockActivityLogService = MockServiceFactory.CreateMockActivityLogService();
        _controller = new AuthController();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        var user = TestDataFactory.CreateTestUser(email: loginDto.Email);
        user.Id = 1;

        var refreshToken = TestDataFactory.CreateTestRefreshToken(user.Id);

        _mockUserService
            .Setup(s => s.GetUserByEmailAsync(loginDto.Email))
            .ReturnsAsync(Result<User>.Success(user));

        _mockUserService
            .Setup(s => s.VerifyPassword(user, loginDto.Password))
            .Returns(true);

        _mockTokenService
            .Setup(s => s.GenerateToken(user.Id, user.Email, user.Username))
            .Returns("jwt-token");

        _mockTokenService
            .Setup(s => s.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockTokenService
            .Setup(s => s.SaveRefreshTokenAsync(user.Id, refreshToken))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.Login(loginDto, _mockUserService.Object, _mockTokenService.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        _mockTokenService.Verify(s => s.ApplyTokensToResponse(
            It.IsAny<HttpResponse>(),
            "jwt-token",
            refreshToken.Token), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123"
        };

        _mockUserService
            .Setup(s => s.GetUserByEmailAsync(loginDto.Email))
            .ReturnsAsync(Result<User>.NotFound("User not found"));

        // Act
        var result = await _controller.Login(loginDto, _mockUserService.Object, _mockTokenService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, objectResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = TestDataFactory.CreateTestUser(email: loginDto.Email);

        _mockUserService
            .Setup(s => s.GetUserByEmailAsync(loginDto.Email))
            .ReturnsAsync(Result<User>.Success(user));

        _mockUserService
            .Setup(s => s.VerifyPassword(user, loginDto.Password))
            .Returns(false);

        // Act
        var result = await _controller.Login(loginDto, _mockUserService.Object, _mockTokenService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, objectResult.StatusCode);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Username = "newuser",
            Password = "Password123"
        };

        var createdUser = TestDataFactory.CreateTestUser(
            email: registerDto.Email,
            username: registerDto.Username);
        createdUser.Id = 1;

        _mockUserService
            .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(Result<User>.Success(createdUser));

        // Act
        var result = await _controller.Register(registerDto, _mockUserService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, objectResult.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Username = "newuser",
            Password = "Password123"
        };

        _mockUserService
            .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(Result<User>.Failure("Email already exists", 400));

        // Act
        var result = await _controller.Register(registerDto, _mockUserService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task Logout_RemovesTokensAndRevokesRefreshToken()
    {
        // Arrange
        var refreshToken = "refresh-token-value";

        _mockTokenService
            .Setup(s => s.GetRefreshTokenFromRequest(It.IsAny<HttpRequest>()))
            .Returns(refreshToken);

        _mockTokenService
            .Setup(s => s.RevokeRefreshTokenAsync(refreshToken))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.Logout(_mockTokenService.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        _mockTokenService.Verify(s => s.RevokeRefreshTokenAsync(refreshToken), Times.Once);
        _mockTokenService.Verify(s => s.RemoveTokensFromResponse(It.IsAny<HttpResponse>()), Times.Once);
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_StillSucceeds()
    {
        // Arrange
        _mockTokenService
            .Setup(s => s.GetRefreshTokenFromRequest(It.IsAny<HttpRequest>()))
            .Returns((string?)null);

        // Act
        var result = await _controller.Logout(_mockTokenService.Object);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockTokenService.Verify(s => s.RevokeRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidateToken_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var token = "valid-jwt-token";

        _mockTokenService
            .Setup(s => s.GetTokenFromRequest(It.IsAny<HttpRequest>()))
            .Returns(token);

        _mockTokenService
            .Setup(s => s.ValidateTokenAsync(token))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateToken(_mockTokenService.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ValidateToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var token = "invalid-jwt-token";

        _mockTokenService
            .Setup(s => s.GetTokenFromRequest(It.IsAny<HttpRequest>()))
            .Returns(token);

        _mockTokenService
            .Setup(s => s.ValidateTokenAsync(token))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateToken(_mockTokenService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, objectResult.StatusCode);
    }

    [Fact]
    public async Task ValidateToken_WithMissingToken_ReturnsUnauthorized()
    {
        // Arrange
        _mockTokenService
            .Setup(s => s.GetTokenFromRequest(It.IsAny<HttpRequest>()))
            .Returns((string?)null);

        // Act
        var result = await _controller.ValidateToken(_mockTokenService.Object);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, objectResult.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var oldRefreshToken = "old-refresh-token";
        var newToken = "new-jwt-token";
        var newRefreshToken = TestDataFactory.CreateTestRefreshToken(1, "new-refresh-token");
        var user = TestDataFactory.CreateTestUser(email: "test@example.com", username: "testuser");
        user.Id = 1;

        _mockTokenService
            .Setup(s => s.GetRefreshTokenFromRequest(It.IsAny<HttpRequest>()))
            .Returns(oldRefreshToken);

        _mockTokenService
            .Setup(s => s.GetRefreshTokenAsync(oldRefreshToken))
            .ReturnsAsync(Result<RefreshToken>.Success(TestDataFactory.CreateTestRefreshToken(1, oldRefreshToken)));

        _mockUserService
            .Setup(s => s.GetUserByIdAsync(1))
            .ReturnsAsync(Result<User>.Success(user));

        _mockTokenService
            .Setup(s => s.RevokeRefreshTokenAsync(oldRefreshToken))
            .ReturnsAsync(Result<bool>.Success(true));

        _mockTokenService
            .Setup(s => s.GenerateToken(1, user.Email, user.Username))
            .Returns(newToken);

        _mockTokenService
            .Setup(s => s.GenerateRefreshToken())
            .Returns(newRefreshToken);

        _mockTokenService
            .Setup(s => s.SaveRefreshTokenAsync(1, newRefreshToken))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.RefreshToken(_mockTokenService.Object, _mockUserService.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockTokenService.Verify(s => s.ApplyTokensToResponse(It.IsAny<HttpResponse>(), newToken, newRefreshToken.Token), Times.Once);
    }
}
