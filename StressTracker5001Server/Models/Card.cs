using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.Models
{
    public class Card
    {
        public int Id { get; set; }
        public required int ColumnId { get; set; }
        public Column? Column { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public required string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Position must be a positive integer")]
        public required int Position { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public int CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
