using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.DTOs.ActivityLog
{
    public class ActivityLogDto
    {
        public required int Id { get; set; }
        public required int BoardId { get; set; }
        public required ActivityLogEntityType EntityType { get; set; }
        public required int EntityId { get; set; }
        public required ActivityLogActionType ActionType { get; set; }
        public required string Description { get; set; }
        public required UserDto CreatedBy { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
