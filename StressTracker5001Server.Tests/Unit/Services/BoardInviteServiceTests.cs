using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
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

        // Create user as owner member (has Admin permissions)
        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Owner);
        _context.BoardMembers.Add(ownerMember);
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
        var result = await _inviteService.RevokeInviteAsync(invite.Id, user.Id);

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

    #region Invite Generation Edge Cases
    [Fact]
    public async Task GenerateInviteAsync_WithMaxInvitesReached_ReturnsFailure()
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

        // Create 10 active invites (max)
        for (int i = 0; i < 10; i++)
        {
            var invite = new BoardInvite
            {
                BoardId = board.Id,
                Token = $"TOKEN{i}",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = false,
                HasBeenUsed = false,
                GeneratedByUserId = user.Id,
                Role = BoardMemberRole.Member,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.BoardInvites.Add(invite);
        }
        await _context.SaveChangesAsync();

        // Act - Try to generate 11th invite
        var result = await _inviteService.GenerateInviteAsync(board.Id, user.Id, BoardMemberRole.Member);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("maximum", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task GenerateInviteAsync_WithMemberRole_ReturnsForbidden()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(user.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Member);
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.GenerateInviteAsync(board.Id, user.Id, BoardMemberRole.Member);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task GenerateInviteAsync_WithNonMember_ReturnsForbidden()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
        var nonMember = TestDataFactory.CreateTestUser(email: "nonmember@example.com");
        _context.Users.AddRange(owner, nonMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.GenerateInviteAsync(board.Id, nonMember.Id, BoardMemberRole.Member);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task GenerateInviteAsync_WithAdminRole_SucceedsAndSetsAdminRole()
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
        var result = await _inviteService.GenerateInviteAsync(board.Id, user.Id, BoardMemberRole.Admin);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BoardMemberRole.Admin, result.Value.Role);
    }

    [Fact]
    public async Task GenerateInviteAsync_WithViewerRole_SucceedsAndSetsViewerRole()
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
        var result = await _inviteService.GenerateInviteAsync(board.Id, user.Id, BoardMemberRole.Viewer);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BoardMemberRole.Viewer, result.Value.Role);
    }
    #endregion

    #region Invite Validation Tests
    [Fact]
    public async Task ValidateInviteCodeAsync_WithRevokedInvite_ReturnsFailure()
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
            IsRevoked = true,
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
        Assert.Contains("revoked", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task ValidateInviteCodeAsync_WithUsedInvite_ReturnsFailure()
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
            HasBeenUsed = true,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = _inviteService.ValidateInviteCodeAsync(invite);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already been used", result.Error ?? string.Empty);
    }
    #endregion

    #region Invite Acceptance Tests
    [Fact]
    public async Task AcceptInviteAsync_WithValidInvite_AddsUserToBoard()
    {
        // Arrange
        var inviter = TestDataFactory.CreateTestUser(email: "inviter@example.com");
        var newUser = TestDataFactory.CreateTestUser(email: "newuser@example.com");
        _context.Users.AddRange(inviter, newUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(inviter.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var inviterMember = TestDataFactory.CreateTestBoardMember(board.Id, inviter.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(inviterMember);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "VALIDTOKEN",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = inviter.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.AcceptInviteAsync(newUser.Id, "VALIDTOKEN");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(board.Id, result.Value.Id);

        // Verify user was added to board
        var member = await _context.BoardMembers
            .FirstOrDefaultAsync(bm => bm.BoardId == board.Id && bm.UserId == newUser.Id);
        Assert.NotNull(member);
        Assert.Equal(BoardMemberRole.Member, member.Role);
    }

    [Fact]
    public async Task AcceptInviteAsync_WithInvalidToken_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.AcceptInviteAsync(user.Id, "INVALIDTOKEN");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task AcceptInviteAsync_WithAlreadyMember_ReturnsFailure()
    {
        // Arrange
        var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
        var existingMember = TestDataFactory.CreateTestUser(email: "member@example.com");
        _context.Users.AddRange(owner, existingMember);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(owner.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, owner.Id, BoardMemberRole.Owner);
        var existingBoardMember = TestDataFactory.CreateTestBoardMember(board.Id, existingMember.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(ownerMember, existingBoardMember);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "TOKEN123",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = owner.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.AcceptInviteAsync(existingMember.Id, "TOKEN123");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("already a member", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task AcceptInviteAsync_WithExpiredInvite_ReturnsFailure()
    {
        // Arrange
        var inviter = TestDataFactory.CreateTestUser(email: "inviter@example.com");
        var newUser = TestDataFactory.CreateTestUser(email: "newuser@example.com");
        _context.Users.AddRange(inviter, newUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(inviter.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var inviterMember = TestDataFactory.CreateTestBoardMember(board.Id, inviter.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(inviterMember);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "EXPIREDTOKEN",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = inviter.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.AcceptInviteAsync(newUser.Id, "EXPIREDTOKEN");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("expired", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task AcceptInviteAsync_MarksInviteAsUsed()
    {
        // Arrange
        var inviter = TestDataFactory.CreateTestUser(email: "inviter@example.com");
        var newUser = TestDataFactory.CreateTestUser(email: "newuser@example.com");
        _context.Users.AddRange(inviter, newUser);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(inviter.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var inviterMember = TestDataFactory.CreateTestBoardMember(board.Id, inviter.Id, BoardMemberRole.Admin);
        _context.BoardMembers.Add(inviterMember);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "MARKTOKEN",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = inviter.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        await _inviteService.AcceptInviteAsync(newUser.Id, "MARKTOKEN");

        // Assert
        var updatedInvite = await _context.BoardInvites.FindAsync(invite.Id);
        Assert.NotNull(updatedInvite);
        Assert.True(updatedInvite.HasBeenUsed);
    }
    #endregion

    #region Invite Revocation Tests
    [Fact]
    public async Task RevokeInviteAsync_WithoutAdminPermission_ReturnsForbidden()
    {
        // Arrange
        var admin = TestDataFactory.CreateTestUser(email: "admin@example.com");
        var member = TestDataFactory.CreateTestUser(email: "member@example.com");
        _context.Users.AddRange(admin, member);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        var memberBoardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(adminMember, memberBoardMember);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "REVOKETOKEN",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = admin.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.RevokeInviteAsync(invite.Id, member.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task RevokeInviteAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.RevokeInviteAsync(9999, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task RevokeInviteAsync_WithAlreadyRevoked_ReturnsFailure()
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

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "ALREADYREVOKED",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true,
            HasBeenUsed = false,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.RevokeInviteAsync(invite.Id, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }
    #endregion

    #region Get Active Invites Tests
    [Fact]
    public async Task GetActiveInvitesForBoardAsync_WithoutAdminPermission_ReturnsForbidden()
    {
        // Arrange
        var admin = TestDataFactory.CreateTestUser(email: "admin@example.com");
        var viewer = TestDataFactory.CreateTestUser(email: "viewer@example.com");
        _context.Users.AddRange(admin, viewer);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        var viewerMember = TestDataFactory.CreateTestBoardMember(board.Id, viewer.Id, BoardMemberRole.Viewer);
        _context.BoardMembers.AddRange(adminMember, viewerMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.GetActiveInvitesForBoardAsync(board.Id, viewer.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task GetActiveInvitesForBoardAsync_FiltersExpiredInvites()
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

        var activeInvite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "ACTIVE",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expiredInvite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "EXPIRED",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.BoardInvites.AddRange(activeInvite, expiredInvite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.GetActiveInvitesForBoardAsync(board.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("ACTIVE", result.Value[0].Token);
    }

    [Fact]
    public async Task GetActiveInvitesForBoardAsync_FiltersRevokedInvites()
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

        var activeInvite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "ACTIVE",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var revokedInvite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "REVOKED",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true,
            HasBeenUsed = false,
            GeneratedByUserId = user.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.BoardInvites.AddRange(activeInvite, revokedInvite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.GetActiveInvitesForBoardAsync(board.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("ACTIVE", result.Value[0].Token);
    }
    #endregion

    #region Revoke All Invites Tests
    [Fact]
    public async Task RevokeAllInvitesForBoardAsync_WithNoActiveInvites_ReturnsFailure()
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
        var result = await _inviteService.RevokeAllInvitesForBoardAsync(board.Id, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task RevokeAllInvitesForBoardAsync_WithAdminPermission_RevokesAllInvites()
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

        // Create 3 invites
        for (int i = 0; i < 3; i++)
        {
            var invite = new BoardInvite
            {
                BoardId = board.Id,
                Token = $"TOKEN{i}",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = false,
                HasBeenUsed = false,
                GeneratedByUserId = user.Id,
                Role = BoardMemberRole.Member,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.BoardInvites.Add(invite);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.RevokeAllInvitesForBoardAsync(board.Id, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        // Verify all invites are revoked
        var activeInvites = _context.BoardInvites
            .Where(bi => bi.BoardId == board.Id && !bi.IsRevoked && bi.ExpiresAt > DateTime.UtcNow)
            .ToList();
        Assert.Empty(activeInvites);
    }

    [Fact]
    public async Task RevokeAllInvitesForBoardAsync_WithoutAdminPermission_ReturnsForbidden()
    {
        // Arrange
        var admin = TestDataFactory.CreateTestUser(email: "admin@example.com");
        var member = TestDataFactory.CreateTestUser(email: "member@example.com");
        _context.Users.AddRange(admin, member);
        await _context.SaveChangesAsync();

        var board = TestDataFactory.CreateTestBoard(admin.Id);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var adminMember = TestDataFactory.CreateTestBoardMember(board.Id, admin.Id, BoardMemberRole.Admin);
        var memberBoardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id, BoardMemberRole.Member);
        _context.BoardMembers.AddRange(adminMember, memberBoardMember);
        await _context.SaveChangesAsync();

        var invite = new BoardInvite
        {
            BoardId = board.Id,
            Token = "TOKEN",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            HasBeenUsed = false,
            GeneratedByUserId = admin.Id,
            Role = BoardMemberRole.Member,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.BoardInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.RevokeAllInvitesForBoardAsync(board.Id, member.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }
    #endregion
}
