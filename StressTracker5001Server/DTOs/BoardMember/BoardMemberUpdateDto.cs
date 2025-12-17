using StressTracker5001Server.Models;

namespace StressTracker5001Server.DTOs.BoardMember
{
    public class BoardMemberUpdateDto
    {
        public required BoardMemberRole Role { get; set; }
    }
}
