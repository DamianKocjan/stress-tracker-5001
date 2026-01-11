using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.Models
{
    public class Attachment
    {
        // Id is a new file name generated when the file is uploaded to storage
        // This helps avoid filename conflicts
        public Guid Id { get; set; }
        public required int CardId { get; set; }
        public Card? Card { get; set; }

        // Original file name
        public required string FileName { get; set; }
        // MIME type of the file
        // e.g., "image/png", "application/pdf"
        public required string ContentType { get; set; }
        public required long FileSize { get; set; }
        public required int UploadedById { get; set; }
        public User? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
