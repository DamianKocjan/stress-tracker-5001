using StressTracker5001Server.DTOs.User;

namespace StressTracker5001Server.DTOs.Card
{
    public class CardDetailsDto
    {
        public required int Id { get; set; }
        public required int ColumnId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required int Position { get; set; }
        public DateTime? DueDate { get; set; }
        public required int CreatedById { get; set; }
        public required UserDto CreatedBy { get; set; }
        public List<int> Tags { get; set; } = new();
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
