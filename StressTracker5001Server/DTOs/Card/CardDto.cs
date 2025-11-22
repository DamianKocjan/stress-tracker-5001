namespace StressTracker5001Server.DTOs.Card
{
    public class CardDto
    {
        public required int Id { get; set; }
        public required int ColumnId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required int Position { get; set; }
        public required DateTimeOffset? DueDate { get; set; }
        public required int CreatedById { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public required DateTimeOffset UpdatedAt { get; set; }
    }
}
