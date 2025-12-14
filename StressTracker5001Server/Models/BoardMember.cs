namespace StressTracker5001Server.Models
{
    public enum BoardMemberRole
    {
        Viewer,
        Member,
        Admin
    }

    public class BoardMember
    {
        public int Id { get; set; }
        public required int BoardId { get; set; }
        public Board? Board { get; set; }
        public required int UserId { get; set; }
        public User? User { get; set; }
        public required BoardMemberRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
