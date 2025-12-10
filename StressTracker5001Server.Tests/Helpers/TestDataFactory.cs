using StressTracker5001Server.Models;

namespace StressTracker5001Server.Tests.Helpers;

public static class TestDataFactory
{
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

    public static Board CreateTestBoard(
        int userId,
        string name = "Test Board",
        string description = "Test Description")
    {
        return new Board
        {
            Name = name,
            Description = description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
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
}
