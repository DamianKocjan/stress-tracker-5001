using StressTracker5001Server.DTOs.User;

namespace StressTracker5001Server.DTOs.Board
{
    public class BoardDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required int OwnerId { get; set; }
        public required UserDto Owner { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public required DateTimeOffset UpdatedAt { get; set; }
    }
}
