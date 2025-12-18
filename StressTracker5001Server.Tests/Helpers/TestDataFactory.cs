using StressTracker5001Server.Models;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Column;

namespace StressTracker5001Server.Tests.Helpers;

public static class TestDataFactory
{
    private static int _userCounter = 0;

    public static User CreateTestUser(
        string email = "test@example.com",
        string username = "testuser",
        string password = "hashedpassword123")
    {
        return new User
        {
            Email = email,
            Username = username,
            Password = password,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test user with Admin role on a board by default
    /// </summary>
    public static User CreateTestAdminUser(string email = "admin@test.com")
    {
        return CreateTestUser(
            email: email,
            username: $"admin_{Interlocked.Increment(ref _userCounter)}",
            password: "admin_password_123"
        );
    }

    /// <summary>
    /// Creates a test user with Member role on a board by default
    /// </summary>
    public static User CreateTestMemberUser(string email = "member@test.com")
    {
        return CreateTestUser(
            email: email,
            username: $"member_{Interlocked.Increment(ref _userCounter)}",
            password: "member_password_123"
        );
    }

    /// <summary>
    /// Creates a test user with Viewer role on a board by default
    /// </summary>
    public static User CreateTestViewerUser(string email = "viewer@test.com")
    {
        return CreateTestUser(
            email: email,
            username: $"viewer_{Interlocked.Increment(ref _userCounter)}",
            password: "viewer_password_123"
        );
    }

    /// <summary>
    /// Creates a test user who is not a member of any board
    /// </summary>
    public static User CreateTestNonMemberUser(string email = "nonmember@test.com")
    {
        return CreateTestUser(
            email: email,
            username: $"nonmember_{Interlocked.Increment(ref _userCounter)}",
            password: "nonmember_password_123"
        );
    }

    public static Board CreateTestBoard(
        int userId,
        string name = "Test Board",
        string description = "Test Description")
    {
        var now = DateTime.UtcNow;
        var board = new Board
        {
            Name = name,
            Description = description,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Note: BoardId will be set by EF after board is saved to database
        // For now, just return the board and create owner member in tests if needed
        return board;
    }

    public static Column CreateTestColumn(
        int boardId,
        string name = "Test Column",
        int position = 0,
        int? wipLimit = null)
    {
        return new Column
        {
            Name = name,
            Position = position,
            WipLimit = wipLimit,
            BoardId = boardId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Card CreateTestCard(
        int columnId,
        string title = "Test Card",
        string description = "Test Card Description",
        int position = 0,
        int? createdById = null)
    {
        return new Card
        {
            Title = title,
            Description = description,
            Position = position,
            ColumnId = columnId,
            CreatedById = createdById ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Tag CreateTestTag(
        int boardId,
        string name = "Test Tag",
        string color = "#FF5733")
    {
        return new Tag
        {
            Name = name,
            Color = color,
            BoardId = boardId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Comment CreateTestComment(
        int cardId,
        int userId,
        string content = "Test Comment")
    {
        return new Comment
        {
            Content = content,
            CardId = cardId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static RefreshToken CreateTestRefreshToken(
        int userId,
        string token = "test-refresh-token",
        DateTime? expiresAt = null)
    {
        return new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static BoardMember CreateTestBoardMember(
        int boardId,
        int userId,
        BoardMemberRole role = BoardMemberRole.Viewer)
    {
        return new BoardMember
        {
            BoardId = boardId,
            UserId = userId,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a CreateColumnDto with provided or default values
    /// </summary>
    public static CreateColumnDto CreateColumnDto(
        string? name = null,
        int? position = null,
        int? wipLimit = null)
    {
        return new CreateColumnDto
        {
            Name = name ?? "Default Column",
            Position = position ?? 0,
            WipLimit = wipLimit
        };
    }

    /// <summary>
    /// Creates a CreateColumnDto with invalid values for testing validation
    /// </summary>
    public static CreateColumnDto CreateInvalidColumnDto(
        string invalidName = "",
        int invalidPosition = -1,
        int? invalidWipLimit = -1)
    {
        return new CreateColumnDto
        {
            Name = invalidName,
            Position = invalidPosition,
            WipLimit = invalidWipLimit
        };
    }

    /// <summary>
    /// Helper to set up a complete board with admin, member, and viewer users
    /// Returns a tuple of (Board, AdminUser, MemberUser, ViewerUser)
    /// Note: Users and board must be added to context and SaveChanges called before using
    /// </summary>
    public static (User AdminUser, User MemberUser, User ViewerUser) CreateBoardRoleUsers()
    {
        var adminUser = CreateTestAdminUser($"admin_{Interlocked.Increment(ref _userCounter)}@test.com");
        var memberUser = CreateTestMemberUser($"member_{Interlocked.Increment(ref _userCounter)}@test.com");
        var viewerUser = CreateTestViewerUser($"viewer_{Interlocked.Increment(ref _userCounter)}@test.com");

        return (adminUser, memberUser, viewerUser);
    }

    /// <summary>
    /// Helper to set up board members in the database with different roles
    /// </summary>
    public static async Task SetupBoardMembersAsync(
        AppDbContext context,
        int boardId,
        User adminUser,
        User memberUser,
        User viewerUser)
    {
        var adminMember = CreateTestBoardMember(boardId, adminUser.Id, BoardMemberRole.Admin);
        var memberMember = CreateTestBoardMember(boardId, memberUser.Id, BoardMemberRole.Member);
        var viewerMember = CreateTestBoardMember(boardId, viewerUser.Id, BoardMemberRole.Viewer);

        context.BoardMembers.AddRange(adminMember, memberMember, viewerMember);
        await context.SaveChangesAsync();
    }
}
