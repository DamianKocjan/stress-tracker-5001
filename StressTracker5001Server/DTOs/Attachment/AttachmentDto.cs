using StressTracker5001Server.DTOs.User;

namespace StressTracker5001Server.DTOs.Attachment
{
    public class AttachmentDto
    {
        public required Guid Id { get; set; }
        public required int CardId { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required long FileSize { get; set; }
        public required int UploadedById { get; set; }
        public required UserDto UploadedBy { get; set; }
        public required DateTime UploadedAt { get; set; }
    }
}
