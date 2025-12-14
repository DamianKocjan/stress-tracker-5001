using StressTracker5001Server.DTOs.BoardMember;

namespace StressTracker5001Server.DTOs.BoardInvite
{
    public class BoardInviteCreateDto
    {
        public required BoardMemberRoleDto Role { get; set; }
        public required DateTime ExpiresAt { get; set; }
    }
}
