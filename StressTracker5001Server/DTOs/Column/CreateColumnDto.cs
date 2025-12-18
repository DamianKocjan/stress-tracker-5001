using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Column
{
    public class CreateColumnDto
    {
        [Required]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Column name must be between 1 and 255 characters")]
        public required string Name { get; set; }

        [Range(0, 1000, ErrorMessage = "Position must be between 0 and 1000")]
        public required int Position { get; set; }

        [Range(0, 1000, ErrorMessage = "WIP limit must be between 0 and 1000")]
        public int? WipLimit { get; set; }
    }
}
