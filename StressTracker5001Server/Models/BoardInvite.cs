
namespace StressTracker5001Server.Models
{
    public class BoardInvite
    {
        public int Id { get; set; }
        public required int BoardId { get; set; }
        public Board? Board { get; set; }
        public required string Token { get; set; }
        // Controls whether the invite is still valid
        public required bool IsRevoked { get; set; }
        public required bool HasBeenUsed { get; set; }
        // Role assigned to users who join via this invite
        public required BoardMemberRole Role { get; set; }
        public required int GeneratedByUserId { get; set; }
        public User? GeneratedByUser { get; set; }
        public required DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
