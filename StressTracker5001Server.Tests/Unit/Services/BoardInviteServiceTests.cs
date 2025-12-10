using Xunit;
using Microsoft.Extensions.Configuration;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class BoardInviteServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BoardAuthorizationService _authService;
    private readonly BoardInviteService _inviteService;
    private readonly IConfiguration _configuration;

    public BoardInviteServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _authService = new BoardAuthorizationService(_context);

        // Create in-memory configuration
        var configData = new Dictionary<string, string?>
    {
      {"BoardInvites:MaxActiveInvitesPerBoard", "10"},
      {"BoardInvites:DefaultInviteExpiryHours", "48"},
      {"BoardInvites:InviteChars", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"},
      {"BoardInvites:InviteTokenLength", "8"}
    };
        _configuration = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

        _inviteService = new BoardInviteService(_context, _configuration, _authService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GenerateInviteAsync_WithValidData_CreatesInvite()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.GenerateInviteAsync(board.Id, user.Id, BoardMemberRole.Member);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.Token);
        Assert.Equal(board.Id, result.Value.BoardId);
        Assert.Equal(user.Id, result.Value.GeneratedByUserId);
        Assert.Equal(BoardMemberRole.Member, result.Value.Role);
    }

    [Fact]
    public async Task GenerateInviteAsync_WithoutAdminPermission_ReturnsForbidden()
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

        // Act
        var result = await _inviteService.GenerateInviteAsync(board.Id, user.Id, BoardMemberRole.Member);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task ValidateInviteCodeAsync_WithValidInvite_ReturnsSuccess()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "TESTTOKEN",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = _inviteService.ValidateInviteCodeAsync(invite);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task ValidateInviteCodeAsync_WithExpiredInvite_ReturnsFailure()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "TESTTOKEN",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = _inviteService.ValidateInviteCodeAsync(invite);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("expired", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task RevokeInviteAsync_WithValidInvite_RevokesInvite()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "TESTTOKEN",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.RevokeInviteAsync(invite.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        // Verify invite was revoked
        var revokedInvite = await _context.BoardInvites.FindAsync(invite.Id);
        Assert.NotNull(revokedInvite);
        Assert.True(revokedInvite.IsRevoked);
    }

    [Fact]
    public async Task GetInviteByCodeAsync_WithValidCode_ReturnsInvite()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "TESTTOKEN",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.GetInviteByCodeAsync("TESTTOKEN");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TESTTOKEN", result.Token);
    }
}
