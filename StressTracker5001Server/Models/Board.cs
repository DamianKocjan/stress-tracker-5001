using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StressTracker5001Server.Models
{
    public class Board
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public required string Name { get; set; }
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public List<Column> Columns { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
        public List<BoardMember> Members { get; set; } = new();
        public List<BoardInvite> Invites { get; set; } = new();
        public List<ActivityLog> ActivityLogs { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
