using StressTracker5001Server.DTOs.User;

namespace StressTracker5001Server.DTOs.BoardInvite
{
    public class BoardInviteDto
    {
        public required int Id { get; set; }
        public required string Token { get; set; }
        public required int Role { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required UserDto GeneratedByUser { get; set; }
    }
}
