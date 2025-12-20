using Xunit;
using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class ActivityLogServiceTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly ActivityLogService _activityLogService;

  public ActivityLogServiceTests()
  {
    _context = TestDbContextFactory.CreateInMemoryDbContext();
    _activityLogService = new ActivityLogService(_context);
  }

  public void Dispose()
  {
    _context.Database.EnsureDeleted();
    _context.Dispose();
  }

  #region LogActivityAsync Tests

  [Fact]
  public async Task LogActivityAsync_WithValidActivity_SavesActivity()
  {
    // Arrange
    var user = TestDataFactory.CreateTestUser();
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(user.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    var activity = new ActivityLog
    {
      BoardId = board.Id,
      UserId = user.Id,
      EntityType = ActivityLogEntityType.Card,
      Action = ActivityLogActionType.Created,
      EntityId = 1,
      Details = "Card created",
      CreatedAt = DateTime.UtcNow
    };

    // Act
    await _activityLogService.LogActivityAsync(activity);

    // Assert
    var savedActivity = await _context.ActivityLogs.FirstOrDefaultAsync();
    Assert.NotNull(savedActivity);
    Assert.Equal(board.Id, savedActivity.BoardId);
    Assert.Equal(ActivityLogEntityType.Card, savedActivity.EntityType);
    Assert.Equal(ActivityLogActionType.Created, savedActivity.Action);
  }

  #endregion

  #region Board Logging Tests

  [Fact]
  public async Task LogBoardCreatedAsync_WithValidData_LogsActivity()
  {
    // Arrange
    var user = TestDataFactory.CreateTestUser();
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(user.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Act
    await _activityLogService.LogBoardCreatedAsync(board.Id, user.Id, "Test Board");

    // Assert
    var activity = await _context.ActivityLogs.FirstOrDefaultAsync();
    Assert.NotNull(activity);
    Assert.Equal(board.Id, activity.BoardId);
    Assert.Equal(ActivityLogEntityType.Board, activity.EntityType);
    Assert.Equal(ActivityLogActionType.Created, activity.Action);
    Assert.Equal(user.Id, activity.UserId);
  }

  [Fact]
  public async Task LogBoardUpdatedAsync_WithValidData_LogsActivityWithDiff()
  {
    // Arrange
    var user = TestDataFactory.CreateTestUser();
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(user.Id, "Old Name", "Old Description");
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    var oldData = new { Name = "Old Name", Description = "Old Description" };
    var newData = new { Name = "New Name", Description = "New Description" };

    // Act
    await _activityLogService.LogBoardUpdatedAsync(board.Id, user.Id, oldData, newData);

    // Assert
    var activity = await _context.ActivityLogs.FirstOrDefaultAsync();
    Assert.NotNull(activity);
    Assert.Equal(board.Id, activity.BoardId);
    Assert.Equal(ActivityLogEntityType.Board, activity.EntityType);
    Assert.Equal(ActivityLogActionType.Updated, activity.Action);
    Assert.Contains("Old Name", activity.Details);
    Assert.Contains("New Name", activity.Details);
  }

  [Fact]
  public async Task LogBoardDeletedAsync_WithValidData_LogsActivity()
  {
    // Arrange
    var user = TestDataFactory.CreateTestUser();
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(user.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Act
    await _activityLogService.LogBoardDeletedAsync(board.Id, user.Id, "Test Board");

    // Assert
    var activity = await _context.ActivityLogs.FirstOrDefaultAsync();
    Assert.NotNull(activity);
    Assert.Equal(ActivityLogEntityType.Board, activity.EntityType);
    Assert.Equal(ActivityLogActionType.Deleted, activity.Action);
  }

  #endregion

  #region Card Assignment Logging Tests

  [Fact]
  public async Task LogCardAssignedAsync_WithValidData_LogsActivity()
  {
    // Arrange
    var user = TestDataFactory.CreateTestUser();
    var assignedUser = TestDataFactory.CreateTestUser("assigned@test.com", "assigned_user");
    _context.Users.AddRange(user, assignedUser);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(user.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Act
    await _activityLogService.LogCardAssignedAsync(board.Id, user.Id, 1, assignedUser.Id, assignedUser.Username);

    // Assert
    var activity = await _context.ActivityLogs.FirstOrDefaultAsync();
    Assert.NotNull(activity);
    Assert.Equal(ActivityLogEntityType.UserAssignment, activity.EntityType);
    Assert.Equal(ActivityLogActionType.Created, activity.Action);
  }

  #endregion

  #region Board Member Logging Tests

  [Fact]
  public async Task LogBoardMemberAddedAsync_WithValidData_LogsActivity()
  {
    // Arrange
    var user = TestDataFactory.CreateTestUser();
    var memberUser = TestDataFactory.CreateTestUser("member@test.com", "member_user");
    _context.Users.AddRange(user, memberUser);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(user.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Act
    await _activityLogService.LogBoardMemberAddedAsync(
        board.Id,
        user.Id,
        memberUser.Id,
        memberUser.Username,
        "Member"
    );

    // Assert
    var activity = await _context.ActivityLogs.FirstOrDefaultAsync();
    Assert.NotNull(activity);
    Assert.Equal(ActivityLogEntityType.BoardMember, activity.EntityType);
    Assert.Equal(ActivityLogActionType.Created, activity.Action);
  }

  [Fact]
  public async Task LogBoardMemberRoleChangedAsync_WithValidData_LogsActivityWithDiff()
  {
    // Arrange
    var user = TestDataFactory.CreateTestUser();
    var memberUser = TestDataFactory.CreateTestUser("member@test.com", "member_user");
    _context.Users.AddRange(user, memberUser);
    await _context.SaveChangesAsync();

    var board = TestDataFactory.CreateTestBoard(user.Id);
    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Act
    await _activityLogService.LogBoardMemberRoleChangedAsync(
        board.Id,
        user.Id,
        memberUser.Id,
        memberUser.Username,
        "Viewer",
        "Member"
    );

    // Assert
    var activity = await _context.ActivityLogs.FirstOrDefaultAsync();
    Assert.NotNull(activity);
    Assert.Equal(ActivityLogEntityType.BoardMember, activity.EntityType);
    Assert.Equal(ActivityLogActionType.Updated, activity.Action);
    Assert.Contains("Viewer", activity.Details);
    Assert.Contains("Member", activity.Details);
  }

  #endregion
}
