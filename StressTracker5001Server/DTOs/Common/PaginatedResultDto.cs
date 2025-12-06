namespace StressTracker5001Server.DTOs.Common
{
    public class PagedResultDto<T>
    {
        public required List<T> Items { get; set; } = new();
        public required bool HasMore { get; set; }
        public required int PreviousPage { get; set; }
        public required int Page { get; set; }
        public required int NextPage { get; set; }
        public required int PageSize { get; set; }
    }
}
