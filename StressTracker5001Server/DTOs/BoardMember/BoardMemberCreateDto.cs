using StressTracker5001Server.Models;

namespace StressTracker5001Server.DTOs.BoardMember
{
    public class BoardMemberCreateDto
    {
        public required int UserId { get; set; }
        public required BoardMemberRole Role { get; set; }
    }
}
