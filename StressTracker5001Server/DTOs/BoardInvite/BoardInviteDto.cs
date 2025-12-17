using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.DTOs.BoardInvite
{
    public class BoardInviteDto
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public required string Token { get; set; }
        public bool IsRevoked { get; set; }
        public bool HasBeenUsed { get; set; }
        public required BoardMemberRole Role { get; set; }
        public int GeneratedByUserId { get; set; }
        public required UserDto GeneratedByUser { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
