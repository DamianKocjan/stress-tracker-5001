namespace StressTracker5001Server.DTOs.Card
{
    public class CreateCardDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset? DueDate { get; set; }
    }
}
