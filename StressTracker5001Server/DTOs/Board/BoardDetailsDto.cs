using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.DTOs.User;

namespace StressTracker5001Server.DTOs.Board
{
    public class BoardDetailsDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required int OwnerId { get; set; }
        public required UserDto Owner { get; set; }
        public required List<ColumnDto> Columns { get; set; } = new();
        public required List<CardDto> Cards { get; set; } = new();
        public required List<TagDto> Tags { get; set; } = new();
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
