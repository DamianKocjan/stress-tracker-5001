namespace StressTracker5001Server.DTOs.BoardMember
{
    public class BoardMemberCreateDto
    {
        public required int UserId { get; set; }
        public required BoardMemberRoleDto Role { get; set; }
    }
}
