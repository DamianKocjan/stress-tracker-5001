namespace StressTracker5001Server.DTOs.Card
{
    public class MoveCardDto
    {
        public required int NewPosition { get; set; }
        public required int NewColumnId { get; set; }
    }
}
