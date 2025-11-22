namespace StressTracker5001Server.DTOs.Column
{
    public class CreateColumnDto
    {
        public required string Name { get; set; }
        public required int Position { get; set; }
        public int? WipLimit { get; set; }
    }
}
