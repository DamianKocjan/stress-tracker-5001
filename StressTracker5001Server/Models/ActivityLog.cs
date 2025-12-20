namespace StressTracker5001Server.Models
{
    public enum ActivityLogEntityType
    {
        Comment,
        UserAssignment,
        Tag,
        Card,
        Column,
        BoardMember,
        Board
    }

    public enum ActivityLogActionType
    {
        Created,
        Updated,
        Deleted,
        Moved
    }

    public class ActivityLog
    {
        public int Id { get; set; }
        public required int BoardId { get; set; }
        public Board? Board { get; set; }
        public required int UserId { get; set; }
        public User? User { get; set; }
        public required ActivityLogEntityType EntityType { get; set; }
        public required int EntityId { get; set; }
        public required ActivityLogActionType Action { get; set; }
        public required string Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
