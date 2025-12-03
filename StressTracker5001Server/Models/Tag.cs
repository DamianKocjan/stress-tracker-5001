using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [StringLength(7, ErrorMessage = "Color cannot exceed 7 characters", MinimumLength = 7)]
        [RegularExpression("^#([0-9A-Fa-f]{6})$", ErrorMessage = "Color must be a valid hex code")]
        public required string Color { get; set; }

        public required int BoardId { get; set; }
        public Board? Board { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
