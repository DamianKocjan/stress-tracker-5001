using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.DTOs.BoardMember
{
    public class BoardMemberDto
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public int UserId { get; set; }
        public required UserDto User { get; set; }
        public required BoardMemberRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
