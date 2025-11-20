using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StressTracker5001Server.Models
{
    public class Column
    {
        public int Id { get; set; }
        public required int BoardId { get; set; }
        public Board? Board { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Position must be a positive integer")]
        public required int Position { get; set; }

        public int? WipLimit { get; set; }

        [JsonIgnore]
        public List<Card> Cards { get; set; } = new();

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
