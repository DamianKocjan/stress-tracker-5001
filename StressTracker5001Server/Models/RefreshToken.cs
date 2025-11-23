using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public required string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !RevokedAt.HasValue && !IsExpired;

        // Foreign key to User
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
