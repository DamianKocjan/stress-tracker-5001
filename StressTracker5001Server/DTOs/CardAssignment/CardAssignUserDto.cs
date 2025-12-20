using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.CardAssignment
{
    public class CardAssignUserDto
    {
        [Required]
        public required int UserId { get; set; }
    }
}
