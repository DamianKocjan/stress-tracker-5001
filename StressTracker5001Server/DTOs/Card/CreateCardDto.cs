namespace StressTracker5001Server.DTOs.Card
{
    public class CreateCardDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
