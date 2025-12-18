using System.ComponentModel.DataAnnotations;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.DTOs.BoardInvite
{
    public class BoardInviteCreateDto
    {
        [Required]
        [EnumDataType(typeof(BoardMemberRole))]
        public required BoardMemberRole Role { get; set; }
    }
}
