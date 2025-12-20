namespace StressTracker5001Server.Models
{
    public class CardAssignment
    {
        public int Id { get; set; }

        public required int CardId { get; set; }
        public Card? Card { get; set; }

        public required int UserId { get; set; }
        public User? User { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
