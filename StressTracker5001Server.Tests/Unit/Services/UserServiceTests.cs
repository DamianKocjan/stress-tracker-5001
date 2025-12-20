using Xunit;
using Moq;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace StressTracker5001Server.Tests.Unit.Services;

public class UserServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserService _userService;
    private readonly IConfiguration _configuration;

    public UserServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();

        // Create in-memory configuration
        var configData = new Dictionary<string, string?>
        {
            {"Auth:TokenChars", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz"},
            {"Auth:TokenLength", "32"},
            {"Auth:PasswordReset:TokenExpiryMinutes", "60"},
            {"Auth:EmailVerification:TokenExpiryMinutes", "1440"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _userService = new UserService(_context, _configuration);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByIdAsync(user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Email, result.Value.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _userService.GetUserByIdAsync(nonExistentId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("not found", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser(email: "test@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByEmailAsync("test@example.com");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Email, result.Value.Email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_IsCaseInsensitive()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser(email: "Test@Example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByEmailAsync("test@example.com");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithInvalidEmail_ReturnsNotFound()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";

        // Act
        var result = await _userService.GetUserByEmailAsync(nonExistentEmail);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = TestDataFactory.CreateTestUser(password: hashedPassword);

        // Act
        var result = _userService.VerifyPassword(user, password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123";
        var wrongPassword = "WrongPassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = TestDataFactory.CreateTestUser(password: hashedPassword);

        // Act
        var result = _userService.VerifyPassword(user, wrongPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_CreatesUser()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Email = "newuser@example.com",
            Username = "newuser",
            Password = "Password123"
        };

        // Act
        var result = await _userService.CreateUserAsync(createDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(createDto.Email, result.Value.Email);
        Assert.Equal(createDto.Username, result.Value.Username);

        // Verify password is hashed
        Assert.NotEqual(createDto.Password, result.Value.Password);
        Assert.True(BCrypt.Net.BCrypt.Verify(createDto.Password, result.Value.Password));
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var existingUser = TestDataFactory.CreateTestUser(email: "existing@example.com");
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var createDto = new CreateUserDto
        {
            Email = "existing@example.com",
            Username = "newuser",
            Password = "Password123"
        };

        // Act
        var result = await _userService.CreateUserAsync(createDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task UpdateUserPasswordAsync_WithValidId_UpdatesPassword()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var newPassword = "NewPassword123";

        // Act
        var result = await _userService.UpdateUserPasswordAsync(user.Id, newPassword);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(BCrypt.Net.BCrypt.Verify(newPassword, result.Value!.Password));
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidId_DeletesUser()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.DeleteUserAsync(user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        // Verify user is deleted
        var deletedUser = await _context.Users.FindAsync(user.Id);
        Assert.Null(deletedUser);
    }
}
