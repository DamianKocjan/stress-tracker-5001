namespace StressTracker5001Server.DTOs.Column
{
    public class ColumnDto
    {
        public required int Id { get; set; }
        public required int BoardId { get; set; }
        public required string Name { get; set; }
        public required int Position { get; set; }
        public int? WipLimit { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public required DateTimeOffset UpdatedAt { get; set; }
    }
}
