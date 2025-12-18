using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Card
{
    public class CardAssignUserDto
    {
        [Required]
        public required int UserId { get; set; }
    }
}
