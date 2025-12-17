using StressTracker5001Server.Models;

namespace StressTracker5001Server.DTOs.BoardInvite
{
    public class BoardInviteCreateDto
    {
        public required BoardMemberRole Role { get; set; }
        public required DateTime ExpiresAt { get; set; }
    }
}
