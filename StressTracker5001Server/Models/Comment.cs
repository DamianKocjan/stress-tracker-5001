using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(5000, ErrorMessage = "Content cannot exceed 5000 characters")]
        public required string Content { get; set; }

        [Required(ErrorMessage = "CardId is required")]
        public required int CardId { get; set; }
        public Card? Card { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public required int UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
