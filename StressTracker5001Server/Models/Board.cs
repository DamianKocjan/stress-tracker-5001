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
        public required int OwnerId { get; set; }
        public User? Owner { get; set; }

        [JsonIgnore]
        public List<Column> Columns { get; set; } = new();

        [JsonIgnore]
        public List<Tag> Tags { get; set; } = new();

        [JsonIgnore]
        public List<BoardMember> Members { get; set; } = new();

        [JsonIgnore]
        public List<BoardInvite> Invites { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
