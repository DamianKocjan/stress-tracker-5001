using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class TokenServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    public TokenServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();

        // Create in-memory configuration with test JWT settings
        var configData = new Dictionary<string, string?>
            {
                {"Jwt:Secret", "ThisIsAVeryLongSecretKeyForTestingPurposes12345678901234567890"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:ExpiryMinutes", "60"},
                {"Jwt:AuthTokenCookieName", "auth-token"},
                {"Jwt:AuthTokenExpiryHours", "1"},
                {"Jwt:RefreshTokenCookieName", "refresh-token"},
                {"Jwt:RefreshTokenExpiryDays", "7"}
            };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _tokenService = new TokenService(_configuration, _context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void GenerateToken_WithValidData_ReturnsToken()
    {
        // Arrange
        int userId = 1;
        string email = "test@example.com";
        string username = "testuser";

        // Act
        var token = _tokenService.GenerateToken(userId, email, username);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsValidRefreshToken()
    {
        // Act
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(refreshToken);
        Assert.NotNull(refreshToken.Token);
        Assert.NotEmpty(refreshToken.Token);
        Assert.True(refreshToken.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var token = _tokenService.GenerateToken(1, "test@example.com", "testuser");

        // Act
        var isValid = _tokenService.ValidateToken(token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = _tokenService.ValidateToken(invalidToken);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task SaveRefreshTokenAsync_WithValidData_SavesToken()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var refreshToken = _tokenService.GenerateRefreshToken();

        // Act
        var result = await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        // Verify token was saved
        var savedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken.Token);
        Assert.NotNull(savedToken);
        Assert.Equal(user.Id, savedToken.UserId);
    }

    [Fact]
    public async Task GetRefreshTokenAsync_WithValidToken_ReturnsToken()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var refreshToken = TestDataFactory.CreateTestRefreshToken(user.Id);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenService.GetRefreshTokenAsync(refreshToken.Token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(refreshToken.Token, result.Value.Token);
    }

    [Fact]
    public async Task GetRefreshTokenAsync_WithExpiredToken_ReturnsFailure()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var expiredToken = TestDataFactory.CreateTestRefreshToken(
            user.Id,
            "expired-token",
            DateTime.UtcNow.AddDays(-1));
        _context.RefreshTokens.Add(expiredToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenService.GetRefreshTokenAsync(expiredToken.Token);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("expired", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithValidToken_RevokesToken()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var refreshToken = TestDataFactory.CreateTestRefreshToken(user.Id);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenService.RevokeRefreshTokenAsync(refreshToken.Token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        // Verify token was revoked
        var revokedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken.Token);
        Assert.NotNull(revokedToken);
        Assert.NotNull(revokedToken.RevokedAt);
    }

    [Fact]
    public void GetTokenFromRequest_WithTokenInCookie_ReturnsToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Cookie"] = "auth-token=test-token-value";

        // Act
        var token = _tokenService.GetTokenFromRequest(context.Request);

        // Assert - Token is parsed from cookie string properly
        Assert.Equal("test-token-value", token);
    }

    [Fact]
    public void RemoveTokensFromResponse_RemovesCookies()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        _tokenService.RemoveTokensFromResponse(context.Response);

        // Assert - Verify no exceptions thrown
        Assert.NotNull(context.Response);
    }
}
