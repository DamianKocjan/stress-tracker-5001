using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Tag;

public class TagCreateDto
{
    [Required(ErrorMessage = "Tag name is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Tag name must be between 1 and 50 characters")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Color is required")]
    [RegularExpression("^#([0-9A-Fa-f]{6})$", ErrorMessage = "Color must be a valid hex color code")]
    public required string Color { get; set; }
    public required int BoardId { get; set; }
}
