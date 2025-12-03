namespace StressTracker5001Server.Models
{
    public class CardTag
    {
        public int CardId { get; set; }
        public Card? Card { get; set; }

        public int TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}