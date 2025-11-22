namespace StressTracker5001Server.DTOs.Card
{
    public class CreateCardDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTimeOffset DueDate { get; set; }
        public required int ColumnId { get; set; }
    }
}
