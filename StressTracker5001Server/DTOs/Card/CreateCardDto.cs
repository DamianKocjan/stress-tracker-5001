using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Card
{
    public class CreateCardDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Card title must be between 1 and 500 characters")]
        public required string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Card description must not exceed 2000 characters")]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
