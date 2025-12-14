using StressTracker5001Server.DTOs.User;

namespace StressTracker5001Server.DTOs.BoardMember
{
    public enum BoardMemberRoleDto
    {
        Viewer,
        Member,
        Admin
    }

    public class BoardMemberDto
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public int UserId { get; set; }
        public required UserDto User { get; set; }
        public required BoardMemberRoleDto Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
