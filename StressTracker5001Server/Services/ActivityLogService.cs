using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Common;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.ActivityLog;
using StressTracker5001Server.DTOs.Common;
using StressTracker5001Server.Models;
using System.Text.Json;

namespace StressTracker5001Server.Services
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(ActivityLog log);

        // Board logging
        Task LogBoardCreatedAsync(int boardId, int userId, string boardName);
        Task LogBoardUpdatedAsync(int boardId, int userId, object oldData, object newData);
        Task LogBoardDeletedAsync(int boardId, int userId, string boardName);

        // Column logging
        Task LogColumnCreatedAsync(int boardId, int userId, int columnId, string columnName);
        Task LogColumnUpdatedAsync(int boardId, int userId, int columnId, object oldData, object newData);
        Task LogColumnMovedAsync(int boardId, int userId, int columnId, int oldPosition, int newPosition);
        Task LogColumnDeletedAsync(int boardId, int userId, int columnId, string columnName);

        // Card logging
        Task LogCardCreatedAsync(int boardId, int userId, int cardId, string cardName);
        Task LogCardUpdatedAsync(int boardId, int userId, int cardId, object oldData, object newData);
        Task LogCardMovedAsync(int boardId, int userId, int cardId, int oldColumnId, int newColumnId);
        Task LogCardDeletedAsync(int boardId, int userId, int cardId, string cardName);

        // Comment logging
        Task LogCommentCreatedAsync(int boardId, int userId, int commentId, int cardId, string content);
        Task LogCommentUpdatedAsync(int boardId, int userId, int commentId, int cardId, string oldContent, string newContent);
        Task LogCommentDeletedAsync(int boardId, int userId, int commentId, int cardId, string content);

        // Tag logging
        Task LogTagCreatedAsync(int boardId, int userId, int tagId, string tagName);
        Task LogTagUpdatedAsync(int boardId, int userId, int tagId, object oldData, object newData);
        Task LogTagDeletedAsync(int boardId, int userId, int tagId, string tagName);
        Task LogCardTagAssignedAsync(int boardId, int userId, int cardId, int tagId, string tagName);
        Task LogCardTagRemovedAsync(int boardId, int userId, int cardId, int tagId, string tagName);

        // Card assignment logging
        Task LogCardAssignedAsync(int boardId, int userId, int cardId, int assignedUserId, string assignedUserName);
        Task LogCardUnassignedAsync(int boardId, int userId, int cardId, int unassignedUserId, string unassignedUserName);

        // Board member logging
        Task LogBoardMemberAddedAsync(int boardId, int userId, int memberId, string memberName, string role);
        Task LogBoardMemberRoleChangedAsync(int boardId, int userId, int memberId, string memberName, string oldRole, string newRole);
        Task LogBoardMemberRemovedAsync(int boardId, int userId, int memberId, string memberName);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly AppDbContext _context;

        public ActivityLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(ActivityLog log)
        {
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a JSON string representing the differences between oldData and newData.
        /// Compares properties and includes only those that have changed.
        /// </summary>
        private string CreateDiffDetails(object oldData, object newData)
        {
            var diff = new Dictionary<string, object>();

            var properties = oldData.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldData);
                var newValue = newData.GetType().GetProperty(prop.Name)?.GetValue(newData);

                // Only include properties that have changed
                if ((oldValue == null && newValue != null) ||
                    (oldValue != null && !oldValue.Equals(newValue)))
                {
                    diff[prop.Name] = new { Old = oldValue, New = newValue };
                }
            }

            return JsonSerializer.Serialize(diff);
        }

        /// <summary>
        /// Creates a JSON string for simple details (name, value, etc.)
        /// </summary>
        private string CreateSimpleDetails(params (string key, object value)[] details)
        {
            var dict = new Dictionary<string, object>();
            foreach (var (key, value) in details)
            {
                dict[key] = value;
            }
            return JsonSerializer.Serialize(dict);
        }

        #region Board Logging

        public async Task LogBoardCreatedAsync(int boardId, int userId, string boardName)
        {
            var details = CreateSimpleDetails(("name", boardName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Board,
                EntityId = boardId,
                Action = ActivityLogActionType.Created,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogBoardUpdatedAsync(int boardId, int userId, object oldData, object newData)
        {
            var details = CreateDiffDetails(oldData, newData);

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Board,
                EntityId = boardId,
                Action = ActivityLogActionType.Updated,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogBoardDeletedAsync(int boardId, int userId, string boardName)
        {
            var details = CreateSimpleDetails(("name", boardName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Board,
                EntityId = boardId,
                Action = ActivityLogActionType.Deleted,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        #endregion

        #region Column Logging

        public async Task LogColumnCreatedAsync(int boardId, int userId, int columnId, string columnName)
        {
            var details = CreateSimpleDetails(("name", columnName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Column,
                EntityId = columnId,
                Action = ActivityLogActionType.Created,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogColumnUpdatedAsync(int boardId, int userId, int columnId, object oldData, object newData)
        {
            var details = CreateDiffDetails(oldData, newData);

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Column,
                EntityId = columnId,
                Action = ActivityLogActionType.Updated,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogColumnMovedAsync(int boardId, int userId, int columnId, int oldPosition, int newPosition)
        {
            var details = CreateSimpleDetails(("oldPosition", oldPosition), ("newPosition", newPosition));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Column,
                EntityId = columnId,
                Action = ActivityLogActionType.Moved,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogColumnDeletedAsync(int boardId, int userId, int columnId, string columnName)
        {
            var details = CreateSimpleDetails(("name", columnName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Column,
                EntityId = columnId,
                Action = ActivityLogActionType.Deleted,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        #endregion

        #region Card Logging

        public async Task LogCardCreatedAsync(int boardId, int userId, int cardId, string cardName)
        {
            var details = CreateSimpleDetails(("name", cardName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Card,
                EntityId = cardId,
                Action = ActivityLogActionType.Created,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogCardUpdatedAsync(int boardId, int userId, int cardId, object oldData, object newData)
        {
            var details = CreateDiffDetails(oldData, newData);

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Card,
                EntityId = cardId,
                Action = ActivityLogActionType.Updated,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogCardMovedAsync(int boardId, int userId, int cardId, int oldColumnId, int newColumnId)
        {
            var details = CreateSimpleDetails(("fromColumnId", oldColumnId), ("toColumnId", newColumnId));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Card,
                EntityId = cardId,
                Action = ActivityLogActionType.Moved,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogCardDeletedAsync(int boardId, int userId, int cardId, string cardName)
        {
            var details = CreateSimpleDetails(("name", cardName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Card,
                EntityId = cardId,
                Action = ActivityLogActionType.Deleted,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        #endregion

        #region Comment Logging

        public async Task LogCommentCreatedAsync(int boardId, int userId, int commentId, int cardId, string content)
        {
            var details = CreateSimpleDetails(("cardId", cardId), ("content", content));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Comment,
                EntityId = commentId,
                Action = ActivityLogActionType.Created,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogCommentUpdatedAsync(int boardId, int userId, int commentId, int cardId, string oldContent, string newContent)
        {
            var details = CreateSimpleDetails(("cardId", cardId), ("oldContent", oldContent), ("newContent", newContent));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Comment,
                EntityId = commentId,
                Action = ActivityLogActionType.Updated,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogCommentDeletedAsync(int boardId, int userId, int commentId, int cardId, string content)
        {
            var details = CreateSimpleDetails(("cardId", cardId), ("content", content));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Comment,
                EntityId = commentId,
                Action = ActivityLogActionType.Deleted,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        #endregion

        #region Tag Logging

        public async Task LogTagCreatedAsync(int boardId, int userId, int tagId, string tagName)
        {
            var details = CreateSimpleDetails(("name", tagName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Tag,
                EntityId = tagId,
                Action = ActivityLogActionType.Created,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogTagUpdatedAsync(int boardId, int userId, int tagId, object oldData, object newData)
        {
            var details = CreateDiffDetails(oldData, newData);

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Tag,
                EntityId = tagId,
                Action = ActivityLogActionType.Updated,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogTagDeletedAsync(int boardId, int userId, int tagId, string tagName)
        {
            var details = CreateSimpleDetails(("name", tagName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Tag,
                EntityId = tagId,
                Action = ActivityLogActionType.Deleted,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogCardTagAssignedAsync(int boardId, int userId, int cardId, int tagId, string tagName)
        {
            var details = CreateSimpleDetails(("tagId", tagId), ("tagName", tagName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Tag,
                EntityId = tagId,
                Action = ActivityLogActionType.Created,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogCardTagRemovedAsync(int boardId, int userId, int cardId, int tagId, string tagName)
        {
            var details = CreateSimpleDetails(("tagId", tagId), ("tagName", tagName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.Tag,
                EntityId = tagId,
                Action = ActivityLogActionType.Deleted,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        #endregion

        #region Card Assignment Logging

        public async Task LogCardAssignedAsync(int boardId, int userId, int cardId, int assignedUserId, string assignedUserName)
        {
            var details = CreateSimpleDetails(("cardId", cardId), ("assignedUserId", assignedUserId), ("assignedUserName", assignedUserName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.UserAssignment,
                EntityId = assignedUserId,
                Action = ActivityLogActionType.Created,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogCardUnassignedAsync(int boardId, int userId, int cardId, int unassignedUserId, string unassignedUserName)
        {
            var details = CreateSimpleDetails(("cardId", cardId), ("unassignedUserId", unassignedUserId), ("unassignedUserName", unassignedUserName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.UserAssignment,
                EntityId = unassignedUserId,
                Action = ActivityLogActionType.Deleted,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        #endregion

        #region Board Member Logging

        public async Task LogBoardMemberAddedAsync(int boardId, int userId, int memberId, string memberName, string role)
        {
            var details = CreateSimpleDetails(("memberId", memberId), ("memberName", memberName), ("role", role));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.BoardMember,
                EntityId = memberId,
                Action = ActivityLogActionType.Created,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogBoardMemberRoleChangedAsync(int boardId, int userId, int memberId, string memberName, string oldRole, string newRole)
        {
            var details = CreateSimpleDetails(("memberId", memberId), ("memberName", memberName), ("oldRole", oldRole), ("newRole", newRole));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.BoardMember,
                EntityId = memberId,
                Action = ActivityLogActionType.Updated,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        public async Task LogBoardMemberRemovedAsync(int boardId, int userId, int memberId, string memberName)
        {
            var details = CreateSimpleDetails(("memberId", memberId), ("memberName", memberName));

            var log = new ActivityLog
            {
                BoardId = boardId,
                UserId = userId,
                EntityType = ActivityLogEntityType.BoardMember,
                EntityId = memberId,
                Action = ActivityLogActionType.Deleted,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogActivityAsync(log);
        }

        #endregion
    }
}
