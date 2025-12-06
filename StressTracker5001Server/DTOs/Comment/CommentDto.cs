using StressTracker5001Server.DTOs.User;

namespace StressTracker5001Server.DTOs.Comment
{
    public class CommentDto
    {
        public required int Id { get; set; }
        public required string Content { get; set; }
        public required int UserId { get; set; }
        public required UserDto User { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
