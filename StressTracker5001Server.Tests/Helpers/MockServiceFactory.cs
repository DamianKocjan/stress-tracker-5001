using Moq;
using StressTracker5001Server.Models;
using StressTracker5001Server.Services;

namespace StressTracker5001Server.Tests.Helpers
{
  /// <summary>
  /// Factory for creating mock dependencies used in tests
  /// </summary>
  public static class MockServiceFactory
  {
    /// <summary>
    /// Creates a mock IActivityLogService that accepts any logging calls without throwing
    /// </summary>
    public static Mock<IActivityLogService> CreateMockActivityLogService()
    {
      var mockActivityLogService = new Mock<IActivityLogService>();

      // Setup all logging methods to complete successfully
      mockActivityLogService
          .Setup(x => x.LogActivityAsync(It.IsAny<ActivityLog>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.GetBoardActivityLogsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
          .ReturnsAsync(new List<ActivityLog>());

      mockActivityLogService
          .Setup(x => x.LogBoardCreatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogBoardUpdatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<object>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogBoardDeletedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogColumnCreatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogColumnUpdatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<object>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogColumnMovedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogColumnDeletedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCardCreatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCardUpdatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<object>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCardMovedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCardDeletedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCommentCreatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCommentUpdatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCommentDeletedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogTagCreatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogTagUpdatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<object>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogTagDeletedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCardTagAssignedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCardTagRemovedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCardAssignedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogCardUnassignedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogBoardMemberAddedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogBoardMemberRoleChangedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      mockActivityLogService
          .Setup(x => x.LogBoardMemberRemovedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      return mockActivityLogService;
    }
  }
}
