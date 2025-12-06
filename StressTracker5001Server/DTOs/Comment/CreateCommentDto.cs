using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Comment
{
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(5000, ErrorMessage = "Content cannot exceed 5000 characters")]
        public required string Content { get; set; }
    }
}
