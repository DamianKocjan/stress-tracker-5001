using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public required int CardId { get; set; }
        public Card? Card { get; set; }

        [Required(ErrorMessage = "FileName is required")]
        public required string FileName { get; set; }
        [Required(ErrorMessage = "FilePath is required")]
        public required string FilePath { get; set; }
        [Required(ErrorMessage = "FileSize is required")]
        public required long FileSize { get; set; }
        public required int UploadedById { get; set; }
        public User? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
