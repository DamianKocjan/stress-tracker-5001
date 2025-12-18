namespace StressTracker5001Server.DTOs.User
{
    public class UserDto
    {
        public required int Id { get; set; }
        public required string Username { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
